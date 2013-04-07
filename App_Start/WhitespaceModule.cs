#region Using

using System;
using System.IO;
using System.Web;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

#endregion

/// <summary>
/// Removes whitespace from the webpage.
/// </summary>
public class WhitespaceModule : IHttpModule
{

    #region IHttpModule Members

    void IHttpModule.Dispose()
    {
        // Nothing to dispose; 
    }

    void IHttpModule.Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(context_BeginRequest);
    }

    #endregion

    void context_BeginRequest(object sender, EventArgs e)
    {
        HttpApplication app = sender as HttpApplication;
        //if (app.Request.RawUrl.EndsWith("/"))
        if (app.Response.ContentType == "text/html")
        {
            app.Response.Filter = new WhitespaceFilter(app.Response.Filter);
        }
    }

    #region Stream filter

    private class WhitespaceFilter : Stream
    {

        public WhitespaceFilter(Stream sink)
        {
            _sink = sink;
            pageContent = new PageContent(HttpContext.Current.Response.ContentEncoding);
        }

        private Stream _sink;
        private static Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}");
        private static Regex commentReg = new Regex("<!--*.*?-->"); // FDJ: Added

        private PageContent pageContent;

        #region Properites

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            bool gzipped = HttpContext.Current.Response.Headers["Content-Encoding"] == "gzip";

            string html = pageContent.GetHtml(gzipped);
            html = reg.Replace(html, string.Empty);
            html = commentReg.Replace(html, string.Empty); // FDJ: Added

            if (gzipped)
            {
                using (GZipStream zipStream = new GZipStream(_sink, CompressionMode.Compress))
                {
                    byte[] outdata = HttpContext.Current.Response.ContentEncoding.GetBytes(html);
                    zipStream.Write(outdata, 0, outdata.GetLength(0));
                }
            }
            else
            {
                byte[] outdata = System.Text.Encoding.Default.GetBytes(html);
                _sink.Write(outdata, 0, outdata.GetLength(0));
            }

            _sink.Flush();
        }

        public override long Length
        {
            get { return 0; }
        }

        private long _position;
        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }

        #endregion

        #region Methods

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _sink.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _sink.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _sink.SetLength(value);
        }

        public override void Close()
        {
            _sink.Close();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);

            pageContent.AddContent(data);

            //string html = System.Text.Encoding.Default.GetString(buffer);
            //string html2 = HttpContext.Current.Response.ContentEncoding.GetString(buffer, offset, count);

            //if (HttpContext.Current.Response.Headers["Content-Encoding"] == "gzip")
            //{
            //    using (Stream s = new MemoryStream(data))
            //    {
            //        using (GZipStream zipStream = new GZipStream(s, CompressionMode.Decompress))
            //        {
            //            byte[] tempBytes = new byte[4 * 4096];
            //            int nRead = zipStream.Read(tempBytes, 0, 4 * 4096);
            //            html2 = HttpContext.Current.Response.ContentEncoding.GetString(tempBytes);
            //        }
            //    }
            //}

            //html2 = reg.Replace(html2, string.Empty);
            //html2 = commentReg.Replace(html2, string.Empty); // FDJ: Added

            //if (HttpContext.Current.Response.Headers["Content-Encoding"] == "gzip")
            //{
            //    using (GZipStream zipStream = new GZipStream(_sink, CompressionMode.Compress))
            //    {
            //        byte[] outdata = HttpContext.Current.Response.ContentEncoding.GetBytes(html2);
            //        zipStream.Write(outdata, 0, outdata.GetLength(0));
            //    }
            //}
            //else
            //{
            //    byte[] outdata = System.Text.Encoding.Default.GetBytes(html2);
            //    _sink.Write(outdata, 0, outdata.GetLength(0));
            //}


            ////html = reg.Replace(html, string.Empty);
            ////html = commentReg.Replace(html, string.Empty); // FDJ: Added

            ////byte[] outdata = System.Text.Encoding.Default.GetBytes(html);
            ////_sink.Write(outdata, 0, outdata.GetLength(0));
        }

        #endregion

    }

    #endregion

    #region PageContent

    class PageBytes
    {
        private List<byte[]> bufferList;

        public PageBytes()
        {
            bufferList = new List<byte[]>();
        }

        public void Add(byte[] bytes)
        {
            bufferList.Add(bytes);
        }

        public byte[] GetBuffer()
        {
            int length = 0;
            foreach (byte[] bufferPart in bufferList)
            {
                length += bufferPart.Length;
            }

            int position = 0;
            byte[] buffer = new byte[length];
            foreach (byte[] bufferPart in bufferList)
            {
                Buffer.BlockCopy(bufferPart, 0, buffer, position, bufferPart.Length);
                position += bufferPart.Length;
            }
            return buffer;
        }

    }

    class PageContent
    {
        private Encoding encoding;
        private PageBytes byteBuffer;

        public PageContent(Encoding enc)
        {
            encoding = enc;
            byteBuffer = new PageBytes();
        }

        public void AddContent(byte[] byteContent)
        {
            byteBuffer.Add(byteContent);
        }

        private byte[] UnZip(byte[] buffer)
        {
            using (Stream s = new MemoryStream(buffer))
            {
                using (GZipStream zipStream = new GZipStream(s, CompressionMode.Decompress))
                {
                    PageBytes unzippedPage = new PageBytes();
                    int bytesRead = 0;
                    do
                    {
                        byte[] tempBytes = new byte[4096];
                        bytesRead = zipStream.Read(tempBytes, 0, 4096);
                        if (bytesRead > 0)
                        {
                            byte[] bytesToAdd = tempBytes;
                            if (tempBytes.Length != bytesRead)
                            {
                                bytesToAdd = new byte[bytesRead];
                                Buffer.BlockCopy(tempBytes, 0, bytesToAdd, 0, bytesRead);
                            }
                            unzippedPage.Add(bytesToAdd);
                        }
                    } while (bytesRead > 0);

                    return unzippedPage.GetBuffer();
                }
            }
        }

        public string GetHtml(bool zipped)
        {
            byte[] buffer = byteBuffer.GetBuffer();
            if (zipped)
            {
                buffer = UnZip(buffer);
            }
            return encoding.GetString(buffer);
        }
    }

    #endregion PageContent
}

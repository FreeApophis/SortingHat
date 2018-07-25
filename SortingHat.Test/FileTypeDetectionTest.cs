﻿using SortingHat.API.FileTypeDetection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SortingHat.Test
{
    public class FileTypeDetectionTest
    {
        private string SignatureResource()
        {
            return "SortingHat.Test.Resources.test.png";
        }

        private Stream TestPngStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(SignatureResource());
        }

        static void DirSearch(string sDir)
        {
            List<string> excluded = new List<string> { ".directory", ".rb", ".txt", ".csv", ".srt", ".ini" };
            try
            {
                var detector = new FileTypeDetector();
                foreach (string f in Directory.EnumerateFiles(sDir, "*.avi", SearchOption.AllDirectories))
                {
                    var fileType = detector.Identify(f);

                    if (fileType == null)
                    {
                        // no filetype ...
                        if (excluded.Any(ext => f.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Console.WriteLine("OK");
                        }
                        else
                        {
                            Console.WriteLine("Unkown Filetype");
                        }
                    }
                    else
                    {
                        if (fileType.Extensions.Any(ext => f.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Console.WriteLine("OK");
                        }
                        else
                        {
                            Console.WriteLine("Unknown Extension");
                        }

                    }
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }


        [Fact]
        public void FindOrphans()
        {
            DirSearch("J:\\Kronos");
        }

        [Fact]
        public void Identify()
        {
            var detector = new FileTypeDetector();

            var result = detector.Identify(TestPngStream());

            Assert.Equal("image", result.Category);
            Assert.Equal("Portable Network Graphics", result.Name);

            Assert.Single(result.Extensions);
            Assert.Equal("png", result.Extensions.First());
        }

    }
}

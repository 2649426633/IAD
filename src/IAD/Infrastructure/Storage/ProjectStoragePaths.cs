using System;
using System.IO;

namespace IAD.Infrastructure.Storage
{
    internal static class ProjectStoragePaths
    {
        public static string RootPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workspace"); }
        }

        public static string DatabasePath { get { return Path.Combine(RootPath, "project.db"); } }
        public static string ImagesPath { get { return Path.Combine(RootPath, "Images"); } }
        public static string MasksPath { get { return Path.Combine(RootPath, "Masks"); } }
        public static string TemplatesPath { get { return Path.Combine(RootPath, "Templates"); } }
        public static string ModelsPath { get { return Path.Combine(RootPath, "Models"); } }
        public static string TrainingRunsPath { get { return Path.Combine(RootPath, "TrainingRuns"); } }
        public static string ResultsPath { get { return Path.Combine(RootPath, "Results"); } }
        public static string LogsPath { get { return Path.Combine(RootPath, "Logs"); } }
        public static string CachePath { get { return Path.Combine(RootPath, "Cache"); } }

        public static void EnsureCreated()
        {
            Directory.CreateDirectory(RootPath);
            Directory.CreateDirectory(ImagesPath);
            Directory.CreateDirectory(MasksPath);
            Directory.CreateDirectory(TemplatesPath);
            Directory.CreateDirectory(ModelsPath);
            Directory.CreateDirectory(TrainingRunsPath);
            Directory.CreateDirectory(ResultsPath);
            Directory.CreateDirectory(LogsPath);
            Directory.CreateDirectory(CachePath);
        }
    }
}

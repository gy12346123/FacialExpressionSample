using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialExpressionSample.Other
{
    class Setting
    {
        /// <summary>
        /// Program base path,with "\" at the last.
        /// </summary>
        public static readonly string BasePath = System.AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Program text language
        /// </summary>
        public static string MainLanguage = ConfigurationManager.AppSettings["MainLanguage"];

        public static string Label = ConfigurationManager.AppSettings["Label"];

        public static string SampleSavePath = ConfigurationManager.AppSettings["SampleSavePath"];

        public static string InfoSavePath = ConfigurationManager.AppSettings["InfoSavePath"];

        public static string FacialCNNModelPath = ConfigurationManager.AppSettings["FacialCNNModelPath"];

        /// <summary>
        /// Reload setting data
        /// </summary>
        public static void Reload()
        {
            ConfigurationManager.RefreshSection("appSettings");
            MainLanguage = ConfigurationManager.AppSettings["MainLanguage"];
            Label = ConfigurationManager.AppSettings["Label"];
            SampleSavePath = ConfigurationManager.AppSettings["SampleSavePath"];
            InfoSavePath = ConfigurationManager.AppSettings["InfoSavePath"];
            FacialCNNModelPath = ConfigurationManager.AppSettings["FacialCNNModelPath"];
        }

        /// <summary>
        /// Read app setting from app.config
        /// </summary>
        /// <param name="keyword">Resource keyword</param>
        /// <returns>Resource</returns>
        public static string ReadAppSetting(string keyword)
        {
            return System.Configuration.ConfigurationManager.AppSettings[keyword];
        }

        /// <summary>
        /// Set app setting to app.config
        /// </summary>
        /// <param name="keyword">Resource keyword</param>
        /// <param name="value">Resource</param>
        /// <param name="needReloadSetting">Reload Setting after set config</param>
        /// <returns></returns>
        public static bool WriteAppSetting(string keyword, string value, bool needReloadSetting = true)
        {
            try
            {
                System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                config.AppSettings.Settings[keyword].Value = value;
                config.Save();
                if (needReloadSetting)
                {
                    Setting.Reload();
                }
                config = null;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}

﻿using MetroFramework.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QCloud.CosApi.Api;
using QCloud.CosApi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 自动截屏并上传
{
    class ImageSG
    {
        /**
        * 
        * @param filepath 图片路径        
        */
        public static string uploadImage(string filepath)
        {
            string res = null;
            try
            {
                var result = "";
                //var bucketName = "bucketName";
                var cos = new CosCloud(AppSettings.APP_ID, AppSettings.SECRET_ID, AppSettings.SECRET_KEY);

                var start = DateTime.Now.ToUniversalTime();

                string remotePath = @"job_hunters/" + Path.GetFileName(filepath);
                //上传文件（不论文件是否分片，均使用本接口）
                var uploadParasDic = new Dictionary<string, string>();
                uploadParasDic.Add(CosParameters.PARA_BIZ_ATTR, "");
                uploadParasDic.Add(CosParameters.PARA_INSERT_ONLY, "0");
                //uploadParasDic.Add(CosParameters.PARA_SLICE_SIZE,SLICE_SIZE.SLIZE_SIZE_3M.ToString());
                result = cos.UploadFile(AppSettings.BUCKET_NAME, remotePath, filepath, uploadParasDic);
                Console.WriteLine("上传文件:" + result);


                var end = DateTime.Now.ToUniversalTime();
                Console.WriteLine(result);
                var obj = (JObject)JsonConvert.DeserializeObject(result);
                var code = (int)obj["code"];
                if (code == 0)
                {
                    var data = obj["data"];
                    res = data["source_url"].ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public static void ClearAll(object metroProgressBar1)
        {
            try
            {
                bool tsop = false;
                int total_count = 0;
                MetroProgressBar metroProgressBar = metroProgressBar1 as MetroProgressBar;
                while (!tsop)
                {
                    var foldListParasDic = new Dictionary<string, string>();
                    foldListParasDic.Add(CosParameters.PARA_NUM, "1000");
                    var cos = new CosCloud(AppSettings.APP_ID, AppSettings.SECRET_ID, AppSettings.SECRET_KEY);
                    //result = cos.DeleteFolder();
                    var result = cos.GetFolderList(AppSettings.BUCKET_NAME, "job_hunters", foldListParasDic);
                    var obj = (JObject)JsonConvert.DeserializeObject(result);
                    var code = (int)obj["code"];
                    if (code == 0)
                    {
                        string data = obj["data"].ToString();//获取message属性(字段)的值  
                        JObject infos = JObject.Parse(data);//把message转化为JObject对象  
                        JArray ja = JArray.Parse(infos["infos"].ToString());//把规格转化为数组对象  
                        int ja_length = ja.Count();//获取数组的长度  
                        metroProgressBar.Maximum = ja_length;
                        for (int i = 0; i < ja_length; i++)
                        {
                            string url = ja[i]["source_url"].ToString();
                            cos.DeleteFile(AppSettings.BUCKET_NAME, "job_hunters" + url.Substring(url.IndexOf("/" + 1)));
                            metroProgressBar.Value += 1;
                        }
                        total_count += ja_length;
                        if (ja_length == 0)
                        {
                            tsop = true;
                        }
                    }
                }
                MessageBox.Show("清理了" + total_count + "个文件");
            }
            catch (Exception ex)
            {

            }
        }
    }
}

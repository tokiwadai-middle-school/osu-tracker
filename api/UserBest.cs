﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;

namespace osu_tracker.api
{
    class UserBest
    {
        List<Score> bestList;
        int user_id;

        public int mainMods;
        public double pp_sum = 0.0;
        public Score newBest = null;

        public double previous_pp_raw, pp_raw;
        public int previous_pp_rank, pp_rank;

        public UserBest(int user_id)
        {
            this.user_id = user_id;

            string userBest = new WebClient().DownloadString(string.Format("https://osu.ppy.sh/api/get_user_best?k={0}&u={1}&limit=100", Program.api_key, user_id)); // api에 베퍼포 정보 요청
            bestList = JsonConvert.DeserializeObject<List<Score>>(userBest); // 베퍼포 100개를 Score 리스트로 변환

            foreach (Score best in bestList)
            {
                pp_sum += best.pp;
            }
        }

        // 새로운 베퍼포
        public void GetNewBest()
        {
            double previous_pp_sum;

            User user = User.Search(user_id);

            pp_raw = user.pp_raw;
            pp_rank = user.pp_rank;

            DataTable userTableSearch = Sql.Get("SELECT user_id FROM pphistories WHERE user_id = {0}", user_id); // 점수 정보에 해당 유저가 있는지 확인
            DataRow ppHistory = Sql.Get("SELECT * FROM pphistories WHERE user_id = {0}", user_id).Rows[0];

            previous_pp_sum = Convert.ToDouble(ppHistory["pp_sum"]);
            previous_pp_raw = Convert.ToDouble(ppHistory["pp_raw"]);
            previous_pp_rank = Convert.ToInt32(ppHistory["pp_rank"]);

            if (!pp_sum.IsCloseTo(previous_pp_sum))
            {
                newBest = bestList.OrderByDescending(
                    x => DateTime.ParseExact(x.date, "yyyy-MM-dd HH:mm:ss", null).AddHours(9)
                ).FirstOrDefault();

                newBest.index = bestList.IndexOf(newBest);
            }

            Sql.Execute("UPDATE pphistories SET pp_sum = {0}, pp_raw = {1}, pp_rank = {2} WHERE user_id = {3}", pp_sum, pp_raw, pp_rank, user_id);
        }

        // 주력 모드
        public void GetMainMods()
        {
            int weight = 0;
            Dictionary<int, double> modList = new Dictionary<int, double>();

            List<Score> halfBestList = bestList.GetRange(0, bestList.Count / 2); // 베퍼포 중 만 검사

            foreach (Score best in halfBestList)
            {
                int mods = best.enabled_mods;
                bool[] modBinary = Convert.ToString(mods, 2).Select(s => s.Equals('1')).ToArray(); // 10진수를 2진 비트 배열로 저장

                // 불필요한 모드 삭제
                for (int i = 1; i <= modBinary.Length; i++)
                {
                    if (modBinary[modBinary.Length - i])
                    {
                        switch (i)
                        {
                            case 1:
                                mods -= 1; // NF
                                break;

                            case 6:
                                mods -= 32; // SD
                                break;

                            case 10:
                                mods -= 512; // NC
                                break;

                            case 13:
                                mods -= 4096; // SO
                                break;

                            case 15:
                                mods -= 16384; // PF
                                break;
                        }
                    }
                }

                double ppWeighted = best.pp * Math.Pow(0.95, weight);

                try
                {
                    modList[mods] += ppWeighted;
                }
                catch
                {
                    modList.Add(mods, ppWeighted);
                }

                weight++;
            }

            mainMods = modList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        }
    }
}
﻿using Discord;
using osu_tracker.api;

namespace osu_tracker.embed
{
    class InfoEmbed : EmbedBuilder
    {
        public InfoEmbed(User user)
        {
            string mainModString;

            if (user.pp_rank == 0)
            {
                mainModString = "없음";
            }
            else
            {
                UserBest userBest = new UserBest(user.user_id);
                userBest.GetMainMods();

                if (userBest.mainMods == 0)
                {
                    mainModString = "노모드";
                }
                else
                {
                    mainModString = userBest.mainMods.ToModString();
                }
            }

            if (user.pp_rank == 0)
            {
                WithDescription("플레이 ​기록이 없는 유저\n\u200B");
                AddField("레벨", string.Format("{0:0.##}", user.level), true);
                AddField("\u200B", "\u200B", true);
            }
            else if (user.pp_raw == 0)
            {
                WithDescription("​장기간 활동이 없는 유저\n\u200B");

                AddField("순위", "#" + user.pp_rank, true);
                AddField("주력 모드", mainModString, true);
                AddField("\u200B", "\u200B", true);
                AddField("정확도", string.Format("{0:0.##}%", user.accuracy), true);
                AddField("레벨", string.Format("{0:0.##}", user.level), true);
            }
            else
            {
                if (user.user_id == 10901226)
                {
                    WithDescription("개발자입니다!\n\u200B");
                }
                else
                {
                    WithDescription("​");
                }

                AddField("퍼포먼스", string.Format("{0:0.##}pp", user.pp_raw), true);
                AddField("주력 모드", mainModString, true);
                AddField("\u200B", "\u200B", true);
                AddField("순위", "#" + user.pp_rank, true);
                AddField("국가 순위", "#" + user.pp_country_rank, true);
                AddField("\u200B", "\u200B", true);
                AddField("정확도", string.Format("{0:0.##}%", user.accuracy), true);
                AddField("레벨", string.Format("{0:0.##}", user.level), true);
            }

            AddField("\u200B", "\u200B", true);
            AddField("플레이 횟수", user.playcount + "회\n\u200B", true);
            AddField("플레이 시간", user.total_seconds_played / 3600 + "시간\u200B", true);
            AddField("\u200B", "\u200B", true);

            WithColor(new Color(0xFF69B4));
            WithTimestamp(user.join_date.ToDateTimeOffset());
            WithFooter(footer => { footer
                .WithText("가입 일시");
            });

            WithThumbnailUrl("https://a.ppy.sh/" + user.user_id);
            WithAuthor(author => { author
                .WithName(user.username)
                .WithUrl("https://osu.ppy.sh/users/" + user.user_id)
                .WithIconUrl("https://www.countryflags.io/" + user.country.ToLower() + "/flat/64.png");
            });
        }
    }
}

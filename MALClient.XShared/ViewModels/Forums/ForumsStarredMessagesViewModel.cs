﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MALClient.Models.Enums;
using MALClient.Models.Models;
using MALClient.Models.Models.Forums;
using MALClient.XShared.NavArgs;

namespace MALClient.XShared.ViewModels.Forums
{
    public class ForumsStarredMessagesViewModel : ViewModelBase
    {
        //count - unique thread
        private Dictionary<MalUser,Tuple<int, List<StarredForumMessage>>> _leaderboard;
        private ICommand _gotoTopicCommand;
        private ICommand _goToProfileCommand;

        public void Init(ForumStarredMessagesNavigationArgs args)
        {
            Leaderboard = new Dictionary<MalUser, Tuple<int, List<StarredForumMessage>>>();
            var data = ResourceLocator.HandyDataStorage.StarredMessages;
            foreach (var user in data.Keys.OrderByDescending(s => data[s].Count))
            {
                Leaderboard.Add(data[user][0].Poster,
                    new Tuple<int, List<StarredForumMessage>>(data[user].Count,
                        data[user].Distinct(StarredForumMessage.TopicIdComparer).ToList()));
            }
            RaisePropertyChanged(() => Leaderboard);
        }


        public Dictionary<MalUser, Tuple<int, List<StarredForumMessage>>> Leaderboard
        {
            get { return _leaderboard; }
            set
            {
                _leaderboard = value;
                RaisePropertyChanged(() => Leaderboard);
            }
        }


        public ICommand GotoTopicCommand
            => _gotoTopicCommand ?? (_gotoTopicCommand = new RelayCommand<StarredForumMessage>(
                   message =>
                   {
                       ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageForumIndex,
                           new ForumStarredMessagesNavigationArgs(message.MessageId));
                       ViewModelLocator.GeneralMain.Navigate(PageIndex.PageForumIndex,
                           new ForumsTopicNavigationArgs(null, long.Parse(message.MessageId)));
                   }));

        public ICommand GoToProfileCommand
            => _goToProfileCommand ?? (_goToProfileCommand = new RelayCommand<StarredForumMessage>(
                   message =>
                   {
                       ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageForumIndex,
                           new ForumStarredMessagesNavigationArgs(message.MessageId));
                       ViewModelLocator.GeneralMain.Navigate(PageIndex.PageProfile,
                           new ProfilePageNavigationArgs
                           {
                               TargetUser = message.Poster.Name
                           });
                   }));

    }
}
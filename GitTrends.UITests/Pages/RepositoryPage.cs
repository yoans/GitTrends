﻿using System.Collections.Generic;
using System.Linq;
using GitTrends.Mobile.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class RepositoryPage : BasePage
    {
        readonly Query _searchBar, _settingsButton, _collectionView, _refreshView,
            _androidContextMenuOverflowButton, _androidSearchBarButton;

        public RepositoryPage(IApp app) : base(app, PageTitles.RepositoryPage)
        {
            _searchBar = GenerateMarkedQuery(RepositoryPageAutomationIds.SearchBar);
            _settingsButton = GenerateMarkedQuery(RepositoryPageAutomationIds.SettingsButton);
            _collectionView = GenerateMarkedQuery(RepositoryPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(RepositoryPageAutomationIds.RefreshView);
            _androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
            _androidSearchBarButton = x => x.Id("ActionSearch");
        }

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public void TapRepository(string repositoryName)
        {
            App.ScrollDownTo(repositoryName);
            App.Tap(repositoryName);

            App.Screenshot($"Tapped {repositoryName}");
        }

        public void EnterSearchBarText(string text)
        {
            if (App.Query(_androidSearchBarButton).Any())
            {
                App.Tap(_androidSearchBarButton);
                App.Screenshot("Tapped Android Search Bar Button");
            }

            App.Tap(_searchBar);
            App.EnterText(text);
            App.DismissKeyboard();
            App.Screenshot($"Entered {text} into Search Bar");
        }

        public void TapSettingsButton()
        {
            if (App.Query(_androidContextMenuOverflowButton).Any())
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Android Overflow Button Tapped");
            }

            App.Tap(_settingsButton);
            App.Screenshot("Settings Button Tapped");
        }

        public void WaitForGitHubUserNotFoundPopup()
        {
            App.WaitForElement(GitHubUserNotFoundConstants.Title);
            App.Screenshot("GitHub User Not Found Popup Appeared");
        }

        public void DeclineGitHubUserNotFoundPopup()
        {
            App.Tap(GitHubUserNotFoundConstants.Decline);
            App.Screenshot("Declined GitHub User Not Found Popup");
        }

        public void AcceptGitHubUserNotFoundPopup()
        {
            App.Tap(GitHubUserNotFoundConstants.Accept);
            App.Screenshot("Accepted GitHub User Not Found Popup");
        }

        public List<Repository> GetVisibleRepositoryList()
        {
            var serializedRepositoryList = App.InvokeBackdoorMethod(BackdoorMethodConstants.GetVisibleCollection).ToString();
            return JsonConvert.DeserializeObject<List<Repository>>(serializedRepositoryList);
        }
    }
}
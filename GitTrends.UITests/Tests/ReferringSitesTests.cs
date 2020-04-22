﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.Demo)]
    class ReferringSitesTests : BaseTest
    {
        public ReferringSitesTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            var referringSites = new List<ReferringSiteModel>();

            var repositories = RepositoryPage.VisibleCollection;
            var repositoriesEnumerator = repositories.GetEnumerator();

            while (!referringSites.Any())
            {
                repositoriesEnumerator.MoveNext();
                RepositoryPage.TapRepository(repositoriesEnumerator.Current.Name);

                await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);
                TrendsPage.TapReferringSitesButton();

                await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);

                referringSites = ReferringSitesPage.VisibleCollection;

                if (!referringSites.Any())
                {
                    ReferringSitesPage.WaitForTheNoReferringSitesDialog();
                    ReferringSitesPage.DismissNoReferringSitesDialog();
                    ReferringSitesPage.ClosePage();

                    await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);
                    TrendsPage.TapBackButton();

                    await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
                }
            }
        }

        [Test]
        public async Task ReferringSitesPageDoesLoad()
        {
            //Arrange
            IReadOnlyCollection<ReferringSiteModel> referringSiteList = ReferringSitesPage.VisibleCollection;
            var referringSite = referringSiteList.First();
            bool isUrlValid = referringSite.IsReferrerUriValid;

            //Act
            if (isUrlValid)
            {
                App.Tap(referringSite.Referrer);
                await Task.Delay(1000).ConfigureAwait(false);
            }

            //Assert
            if (isUrlValid && App is iOSApp)
            {
                SettingsPage.WaitForBrowserToOpen();
                Assert.IsTrue(ReferringSitesPage.IsBrowserOpen);
            }

            Assert.IsTrue(App.Query(referringSite.Referrer).Any());
        }

        [TestCase(ReviewAction.NoButtonTapped, ReviewAction.NoButtonTapped)]
        [TestCase(ReviewAction.NoButtonTapped, ReviewAction.YesButtonTapped)]
        [TestCase(ReviewAction.YesButtonTapped, ReviewAction.NoButtonTapped)]
        [TestCase(ReviewAction.YesButtonTapped, ReviewAction.YesButtonTapped)]
        public void VerifyStoreRequest(ReviewAction firstAction, ReviewAction secondAction)
        {
            //Arrange
            string firstTitleText, secondTitleText, firstNoButtonText, secondNoButtonText, firstYesButtonText, secondYesButtonText;

            //Act
            ReferringSitesPage.TriggerReviewRequest();
            ReferringSitesPage.WaitForReviewRequest();

            firstTitleText = ReferringSitesPage.StoreRatingRequestTitleLabelText;
            firstNoButtonText = ReferringSitesPage.StoreRatingRequestNoButtonText;
            firstYesButtonText = ReferringSitesPage.StoreRatingRequestYesButtonText;

            PerformReviewAction(firstAction);

            secondTitleText = ReferringSitesPage.StoreRatingRequestTitleLabelText;
            secondNoButtonText = ReferringSitesPage.StoreRatingRequestNoButtonText;
            secondYesButtonText = ReferringSitesPage.StoreRatingRequestYesButtonText;

            PerformReviewAction(secondAction);

            ReferringSitesPage.WaitForNoReviewRequest();

            //Assert
            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, firstTitleText);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, firstNoButtonText);
            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, firstYesButtonText);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, secondNoButtonText);
            Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, secondYesButtonText);

            if (firstAction is ReviewAction.NoButtonTapped)
                Assert.AreEqual(ReviewServiceConstants.TitleLabel_Feedback, secondTitleText);
            else
                Assert.AreEqual(ReferringSitesPage.ExpectedAppStoreRequestTitle, secondTitleText);

            if (App is iOSApp && secondAction is ReviewAction.YesButtonTapped)
            {
                if (firstAction is ReviewAction.NoButtonTapped)
                {
                    ReferringSitesPage.WaitForEmailToOpen();
                    Assert.IsTrue(ReferringSitesPage.IsEmailOpen);
                }
                else
                {
                    ReferringSitesPage.WaitForBrowserToOpen();
                    Assert.IsTrue(ReferringSitesPage.IsBrowserOpen);
                }
            }
        }

        void PerformReviewAction(in ReviewAction reviewAction)
        {
            if (reviewAction is ReviewAction.YesButtonTapped)
                ReferringSitesPage.TapStoreRatingRequestYesButton();
            else if (reviewAction is ReviewAction.NoButtonTapped)
                ReferringSitesPage.TapStoreRatingRequestNoButton();
            else
                throw new NotSupportedException();
        }
    }
}

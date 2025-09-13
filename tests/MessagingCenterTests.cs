using Xunit;
using Xunit.Sdk;
using static Plugin.Maui.MessagingCenter.MessagingCenter;

namespace Plugin.Maui.MessagingCenter.UnitTests
{
	public class MessagingCenterTests
	{
		TestSubcriber _subscriber;

		[Fact]
		public void SingleSubscriber()
		{
			string sentMessage = null;
			Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);

			Send(this, "SimpleTest", "My Message");

			Assert.Equal("My Message", sentMessage);

			Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");
		}

		[Fact]
		public void Filter()
		{
			string sentMessage = null;
			Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args, this);

			Send(new MessagingCenterTests(), "SimpleTest", "My Message");

			Assert.Null(sentMessage);

			Send(this, "SimpleTest", "My Message");

			Assert.Equal("My Message", sentMessage);

			Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");
		}

		[Fact]
		public void MultiSubscriber()
		{
			var sub1 = new object();
			var sub2 = new object();
			string sentMessage1 = null;
			string sentMessage2 = null;
			Subscribe<MessagingCenterTests, string>(sub1, "SimpleTest", (sender, args) => sentMessage1 = args);
			Subscribe<MessagingCenterTests, string>(sub2, "SimpleTest", (sender, args) => sentMessage2 = args);

			Send(this, "SimpleTest", "My Message");

			Assert.Equal("My Message", sentMessage1);
			Assert.Equal("My Message", sentMessage2);

			Unsubscribe<MessagingCenterTests, string>(sub1, "SimpleTest");
			Unsubscribe<MessagingCenterTests, string>(sub2, "SimpleTest");
		}

		[Fact]
		public void Unsubscribe()
		{
			string sentMessage = null;
			Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);
			Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");

			Send(this, "SimpleTest", "My Message");

			Assert.Null(sentMessage);
		}

		[Fact]
		public void SendWithoutSubscribers()
		{
			Send(this, "SimpleTest", "My Message");
		}

		[Fact]
		public void NoArgSingleSubscriber()
		{
			bool sentMessage = false;
			Subscribe<MessagingCenterTests>(this, "SimpleTest", sender => sentMessage = true);

			Send(this, "SimpleTest");

			Assert.True(sentMessage);

			Unsubscribe<MessagingCenterTests>(this, "SimpleTest");
		}

		[Fact]
		public void NoArgFilter()
		{
			bool sentMessage = false;
			Subscribe(this, "SimpleTest", (sender) => sentMessage = true, this);

			Send(new MessagingCenterTests(), "SimpleTest");

			Assert.False(sentMessage);

			Send(this, "SimpleTest");

			Assert.True(sentMessage);

			Unsubscribe<MessagingCenterTests>(this, "SimpleTest");
		}

		[Fact]
		public void NoArgMultiSubscriber()
		{
			var sub1 = new object();
			var sub2 = new object();
			bool sentMessage1 = false;
			bool sentMessage2 = false;
			Subscribe<MessagingCenterTests>(sub1, "SimpleTest", (sender) => sentMessage1 = true);
			Subscribe<MessagingCenterTests>(sub2, "SimpleTest", (sender) => sentMessage2 = true);

			Send(this, "SimpleTest");

			Assert.True(sentMessage1);
			Assert.True(sentMessage2);

			Unsubscribe<MessagingCenterTests>(sub1, "SimpleTest");
			Unsubscribe<MessagingCenterTests>(sub2, "SimpleTest");
		}

		[Fact]
		public void NoArgUnsubscribe()
		{
			bool sentMessage = false;
			Subscribe<MessagingCenterTests>(this, "SimpleTest", (sender) => sentMessage = true);
			Unsubscribe<MessagingCenterTests>(this, "SimpleTest");

			Send(this, "SimpleTest", "My Message");

			Assert.False(sentMessage);
		}

		[Fact]
		public void NoArgSendWithoutSubscribers()
		{
			Send(this, "SimpleTest");
		}

		[Fact]
		public void ThrowOnNullArgs()
		{
			Assert.Throws<ArgumentNullException>(() => Subscribe<MessagingCenterTests, string>(null, "Foo", (sender, args) => { }));
			Assert.Throws<ArgumentNullException>(() => Subscribe<MessagingCenterTests, string>(this, null, (sender, args) => { }));
			Assert.Throws<ArgumentNullException>(() => Subscribe<MessagingCenterTests, string>(this, "Foo", null));

			Assert.Throws<ArgumentNullException>(() => Subscribe<MessagingCenterTests>(null, "Foo", (sender) => { }));
			Assert.Throws<ArgumentNullException>(() => Subscribe<MessagingCenterTests>(this, null, (sender) => { }));
			Assert.Throws<ArgumentNullException>(() => Subscribe<MessagingCenterTests>(this, "Foo", null));

			Assert.Throws<ArgumentNullException>(() => Send<MessagingCenterTests, string>(null, "Foo", "Bar"));
			Assert.Throws<ArgumentNullException>(() => Send<MessagingCenterTests, string>(this, null, "Bar"));

			Assert.Throws<ArgumentNullException>(() => Send<MessagingCenterTests>(null, "Foo"));
			Assert.Throws<ArgumentNullException>(() => Send<MessagingCenterTests>(this, null));

			Assert.Throws<ArgumentNullException>(() => Unsubscribe<MessagingCenterTests>(null, "Foo"));
			Assert.Throws<ArgumentNullException>(() => Unsubscribe<MessagingCenterTests>(this, null));

			Assert.Throws<ArgumentNullException>(() => Unsubscribe<MessagingCenterTests, string>(null, "Foo"));
			Assert.Throws<ArgumentNullException>(() => Unsubscribe<MessagingCenterTests, string>(this, null));
		}

		[Fact]
		public void UnsubscribeInCallback()
		{
			int messageCount = 0;

			var subscriber1 = new object();
			var subscriber2 = new object();

			Subscribe<MessagingCenterTests>(subscriber1, "SimpleTest", (sender) =>
			{
				messageCount++;
				Unsubscribe<MessagingCenterTests>(subscriber2, "SimpleTest");
			});

			Subscribe<MessagingCenterTests>(subscriber2, "SimpleTest", (sender) =>
			{
				messageCount++;
				Unsubscribe<MessagingCenterTests>(subscriber1, "SimpleTest");
			});

			Send(this, "SimpleTest");

			Assert.Equal(1, messageCount);
		}

		[Fact]
		public void SubscriberShouldBeCollected()
		{
			new Action(() =>
			{
				var subscriber = new TestSubcriber();
				Subscribe<TestPublisher>(subscriber, "test", p => throw new XunitException("The subscriber should have been collected."));
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test(); // Assert.Fail() shouldn't be called, because the TestSubcriber object should have ben GCed
		}

		[Fact]
		public void ShouldBeCollectedIfCallbackTargetIsSubscriber()
		{
			WeakReference wr = null;

			new Action(() =>
			{
				var subscriber = new TestSubcriber();

				wr = new WeakReference(subscriber);

				subscriber.SubscribeToTestMessages();
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test();

			Assert.False(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
		}

		[Fact]
		public void NotCollectedIfSubscriberIsNotTheCallbackTarget()
		{
			WeakReference wr = null;

			new Action(() =>
			{
				var subscriber = new TestSubcriber();

				wr = new WeakReference(subscriber);

				// This creates a closure, so the callback target is not 'subscriber', but an instancce of a compiler generated class 
				// So MC has to keep a strong reference to it, and 'subscriber' won't be collectable
				Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.True(wr.IsAlive); // The closure in Subscribe should be keeping the subscriber alive
			Assert.NotNull(wr.Target as TestSubcriber);

			Assert.False(((TestSubcriber)wr.Target).Successful);

			var pub = new TestPublisher();
			pub.Test();

			Assert.True(((TestSubcriber)wr.Target).Successful);  // Since it's still alive, the subscriber should still have received the message and updated the property
		}

		[Fact]
		public void SubscriberCollectableAfterUnsubscribeEvenIfHeldByClosure()
		{
			WeakReference CreateReference()
			{
				WeakReference wr = null;

				new Action(() =>
				{
					var subscriber = new TestSubcriber();

					wr = new WeakReference(subscriber);

					Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
				})();

				Assert.NotNull(wr.Target as TestSubcriber);

				Unsubscribe<TestPublisher>(wr.Target, "test");

				return wr;
			}

			var wr = CreateReference();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
		}

		[Fact]
		public void StaticCallback()
		{
			int i = 4;

			_subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

			Subscribe<TestPublisher>(_subscriber, "test", p => MessagingCenterTestsCallbackSource.Increment(ref i));

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test();

			Assert.True(i == 5, "The static method should have incremented 'i'");
		}

		[Fact]
		public void NothingShouldBeCollected()
		{
			var success = false;

			_subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

			var source = new MessagingCenterTestsCallbackSource();
			Subscribe<TestPublisher>(_subscriber, "test", p => source.SuccessCallback(ref success));

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test();

			Assert.True(success); // TestCallbackSource.SuccessCallback() should be invoked to make success == true
		}

		[Fact]
		public void MultipleSubscribersOfTheSameClass()
		{
			var sub1 = new object();
			var sub2 = new object();

			string args2 = null;

			const string message = "message";

			Subscribe<MessagingCenterTests, string>(sub1, message, (sender, args) => { });
			Subscribe<MessagingCenterTests, string>(sub2, message, (sender, args) => args2 = args);
			Unsubscribe<MessagingCenterTests, string>(sub1, message);

			Send(this, message, "Testing");
			Assert.True(args2 == "Testing", "unsubscribing sub1 should not unsubscribe sub2");
		}

		[Fact]
		public void SubscribeSendUnsubscribeSubscribeSend()
		{
			var subscriber = new TestSubcriber();
			var pub = new TestPublisher();
			
			// First subscription and message
			Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
			
			Assert.False(subscriber.Successful);
			pub.Test();
			Assert.True(subscriber.Successful);
			
			// Reset and unsubscribe
			subscriber.Reset();
			Unsubscribe<TestPublisher>(subscriber, "test");
			
			// Verify unsubscribed - message should not be received
			pub.Test();
			Assert.False(subscriber.Successful);
			
			// Subscribe again
			Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
			
			// Send message again - this should work
			pub.Test();
			Assert.True(subscriber.Successful);
		}

	[Fact]
	public void DuplicateSubscriptionThrowsException()
	{
		var subscriber = new object();
		
		// First subscription should work
		Subscribe<MessagingCenterTests, string>(subscriber, "test", (sender, args) => { });
		
		// Second subscription to the same message type should throw
		var exception = Assert.Throws<InvalidOperationException>(() =>
			Subscribe<MessagingCenterTests, string>(subscriber, "test", (sender, args) => { }));
			
		Assert.Contains("already subscribed", exception.Message);
		
		// Clean up
		Unsubscribe<MessagingCenterTests, string>(subscriber, "test");
	}

	[Fact]
	public void MultipleSubscriptionsWithDifferentMessagesAllowed()
	{
		// This test replicates the user's scenario where they subscribe to multiple different messages
		var subscriber = new object();
		
		bool message1Received = false;
		bool message2Received = false;
		bool message3Received = false;
		
		// Subscribe to three different messages (like the user's code)
		Subscribe<MessageModel>(subscriber, "VerbeteFavorited123", (sender) => message1Received = true);
		Subscribe<MessageModel>(subscriber, "FavoriteVerbeteDeleted456", (sender) => message2Received = true);
		Subscribe<MessageModel>(subscriber, "GotoCatGramOpenClose", (sender) => message3Received = true);
		
		// Send all three messages
		Send(new MessageModel(), "VerbeteFavorited123");
		Send(new MessageModel(), "FavoriteVerbeteDeleted456");
		Send(new MessageModel(), "GotoCatGramOpenClose");
		
		// All should be received
		Assert.True(message1Received);
		Assert.True(message2Received);
		Assert.True(message3Received);
		
		// Clean up
		Unsubscribe<MessageModel>(subscriber, "VerbeteFavorited123");
		Unsubscribe<MessageModel>(subscriber, "FavoriteVerbeteDeleted456");
		Unsubscribe<MessageModel>(subscriber, "GotoCatGramOpenClose");
	}

	// Helper class to match the user's MessageModel
	public class MessageModel
	{
	}

		class TestSubcriber
		{
			public void SetSuccess()
			{
				Successful = true;
			}

			public void Reset()
			{
				Successful = false;
			}

			public bool Successful { get; private set; }

			public void SubscribeToTestMessages()
			{
				Subscribe<TestPublisher>(this, "test", p => SetSuccess());
			}
		}

		class TestPublisher
		{
			public void Test()
			{
				Send(this, "test");
			}
		}

		public class MessagingCenterTestsCallbackSource
		{
			public void SuccessCallback(ref bool success)
			{
				success = true;
			}

			public static void Increment(ref int i)
			{
				i = i + 1;
			}
		}

		[Fact]
		public void TestMessagingCenterSubstitute()
		{
			var mc = new FakeMessagingCenter();

			// In the real world, you'd construct this with `new ComponentWithMessagingDependency(MessagingCenter.Instance);`
			var component = new ComponentWithMessagingDependency(mc);
			component.DoAThing();

			Assert.True(mc.WasSubscribeCalled, "ComponentWithMessagingDependency should have subscribed in its constructor");
			Assert.True(mc.WasSendCalled, "The DoAThing method should send a message");
		}

		class ComponentWithMessagingDependency
		{
			readonly IMessagingCenter _messagingCenter;

			public ComponentWithMessagingDependency(IMessagingCenter messagingCenter)
			{
				_messagingCenter = messagingCenter;
				_messagingCenter.Subscribe<ComponentWithMessagingDependency>(this, "test", dependency => Console.WriteLine("test"));
			}

			public void DoAThing()
			{
				_messagingCenter.Send(this, "test");
			}
		}

		internal class FakeMessagingCenter : IMessagingCenter
		{
			public bool WasSubscribeCalled { get; private set; }
			public bool WasSendCalled { get; private set; }

			public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
			{
				WasSendCalled = true;
			}

			public void Send<TSender>(TSender sender, string message) where TSender : class
			{
				WasSendCalled = true;
			}

			public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = default(TSender)) where TSender : class
			{
				WasSubscribeCalled = true;
			}

			public void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = default(TSender)) where TSender : class
			{
				WasSubscribeCalled = true;
			}

			public void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
			{

			}

			public void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
			{

			}
		}
	}
}
using System.Collections.Generic;
using EventStore.Core.Data;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;
using EventStore.Core.Services.RequestManager;
using EventStore.Core.Services.RequestManager.Managers;
using EventStore.Core.Tests.Fakes;
using EventStore.Core.Tests.Helpers;
using NUnit.Framework;

namespace EventStore.Core.Tests.Services.Replication.WriteStream {
	[TestFixture]
	public class when_write_stream_gets_already_committed_and_log_is_not_committed : RequestManagerSpecification {
		private long _commitLogPosition = 1000;
		protected override IRequestManager OnManager(FakePublisher publisher) {
			return new WriteStreamRequestManager(publisher,  CommitTimeout, false);
		}

		protected override IEnumerable<Message> WithInitialMessages() {
			yield return new ClientMessage.WriteEvents(InternalCorrId, ClientCorrId, Envelope, true, "test123",
				ExpectedVersion.Any, new[] { DummyEvent() }, null);
			yield return new CommitMessage.CommittedTo(_commitLogPosition - 1);
		}

		protected override Message When() {
			return new StorageMessage.AlreadyCommitted(InternalCorrId, "test123", 0, 1, _commitLogPosition);
		}

		[Test]
		public void successful_request_message_is_not_published() {
			Assert.That(Produced.IsEmpty());
		}

		[Test]
		public void the_envelope_is_not_replied_to() {
			Assert.That(Envelope.Replies.IsEmpty());
		}
	}
}

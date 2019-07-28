//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace RedisRpc.Models {
//	public class DeliveredMessage1 {
//		private readonly DeliveredMessage.OffsetAndLength[] binariesInfo;

//		public Properties Properties { get; private set; }

//		public string ConsumerTag { get; private set; }

//		public ulong DeliveryTag { get; private set; }

//		public bool Redelivered { get; private set; }

//		public string Exchange { get; private set; }

//		public string RoutingKey { get; private set; }

//		public byte[] Body { get; private set; }

//		public bool HasBinary {
//			get {
//				return ((IEnumerable<DeliveredMessage.OffsetAndLength>)this.binariesInfo).Any<DeliveredMessage.OffsetAndLength>();
//			}
//		}

//		public CancellationToken CancelToken { get; }

//		public DeliveredMessage(
//		  string consumerTag,
//		  ulong deliveryTag,
//		  bool redelivered,
//		  string exchange,
//		  string routingKey,
//		  Properties properties,
//		  byte[] body,
//		  Func<VerificationResult> signatureCheck,
//		  Func<Permission> permissionCheck,
//		  CancellationToken? cancelToken = null) {
//			this.ConsumerTag = consumerTag;
//			this.DeliveryTag = deliveryTag;
//			this.Redelivered = redelivered;
//			this.Exchange = exchange;
//			this.RoutingKey = routingKey;
//			this.Properties = properties;
//			this.Body = body;
//			this.CancelToken = cancelToken ?? CancellationToken.None;
//			this.signatureVerification = new Lazy<VerificationResult>(signatureCheck, LazyThreadSafetyMode.PublicationOnly);
//			this.permission = new Lazy<Permission>(permissionCheck, LazyThreadSafetyMode.PublicationOnly);
//			if (this.Properties.Headers != null && this.Properties.Headers.ContainsKey(Headers.BinaryOffsets)) {
//				int[] array = ((IEnumerable)this.Properties.Headers[Headers.BinaryOffsets]).Cast<int>().Concat<int>((IEnumerable<int>)new int[1]
//				{
//		  this.Body.Length
//				}).ToArray<int>();
//				this.binariesInfo = new DeliveredMessage.OffsetAndLength[array.Length - 1];
//				for (int index = 0; index < array.Length - 1; ++index)
//					this.binariesInfo[index] = new DeliveredMessage.OffsetAndLength() {
//						Length = array[index + 1] - array[index],
//						Offset = array[index]
//					};
//			} else
//				this.binariesInfo = Array.Empty<DeliveredMessage.OffsetAndLength>();
//		}

//		public string GetTopic() {
//			return this.Properties.GetHeaderValue<string>(Headers.Topic, this.RoutingKey);
//		}

//		public DateTime GetCreatedAt() {
//			return this.Properties.GetCreatedAt();
//		}

//		public Guid GetUuid() {
//			string messageId = this.Properties.MessageId;
//			Guid result;
//			if (string.IsNullOrEmpty(messageId) || !Guid.TryParse(messageId, out result))
//				return Guid.Empty;
//			return result;
//		}

//		public string GetAppId() {
//			return this.Properties.AppId;
//		}

//		public string GetHubVersion() {
//			return this.Properties.GetHeaderValue<string>(Headers.HubVersion, "");
//		}

//		public string GetRawContent() {
//			int count = this.Body.Length;
//			if (this.HasBinary)
//				count = this.binariesInfo[0].Offset;
//			return Encoding.UTF8.GetString(this.Body, 0, count);
//		}

//		public Stream GetBinaryStream() {
//			if (!this.HasBinary)
//				throw new InvalidOperationException();
//			return (Stream)new MemoryStream(this.Body, this.binariesInfo[0].Offset, this.Body.Length - this.binariesInfo[0].Offset, false);
//		}

//		public Stream GetBinaryStream(int index) {
//			if (!this.HasBinary)
//				throw new InvalidOperationException();
//			if (index < 0 || index >= this.binariesInfo.Length)
//				throw new IndexOutOfRangeException();
//			return (Stream)new MemoryStream(this.Body, this.binariesInfo[index].Offset, this.binariesInfo[index].Length, false);
//		}

//		public byte[] GetBinary() {
//			if (!this.HasBinary)
//				throw new InvalidOperationException("No binary part in message");
//			int offset = this.binariesInfo[0].Offset;
//			byte[] numArray = new byte[this.Body.Length - offset];
//			Buffer.BlockCopy((Array)this.Body, offset, (Array)numArray, 0, numArray.Length);
//			return numArray;
//		}

//		public byte[] GetBinary(int index) {
//			if (!this.HasBinary)
//				throw new InvalidOperationException("No binary part in message");
//			if (index < 0 || index >= this.binariesInfo.Length)
//				throw new IndexOutOfRangeException();
//			DeliveredMessage.OffsetAndLength offsetAndLength = this.binariesInfo[index];
//			byte[] numArray = new byte[offsetAndLength.Length];
//			Buffer.BlockCopy((Array)this.Body, offsetAndLength.Offset, (Array)numArray, 0, offsetAndLength.Length);
//			return numArray;
//		}

//		public int[] GetBinariesLengths() {
//			return ((IEnumerable<DeliveredMessage.OffsetAndLength>)this.binariesInfo).Select<DeliveredMessage.OffsetAndLength, int>((Func<DeliveredMessage.OffsetAndLength, int>)(i => i.Length)).ToArray<int>();
//		}

//		public int GetBinariesCount() {
//			return this.binariesInfo.Length;
//		}

//		public X509Certificate2 GetCertificate() {
//			byte[] headerValue = this.Properties.GetHeaderValue<byte[]>(Headers.Certificate, (byte[])null);
//			if (headerValue == null)
//				return (X509Certificate2)null;
//			return new X509Certificate2(headerValue);
//		}

//		public VerificationResult Verify() {
//			return this.signatureVerification.Value;
//		}

//		public Permission CheckPermission() {
//			return this.permission.Value;
//		}

//		private struct OffsetAndLength {
//			public int Offset;
//			public int Length;
//		}
//	}
//}

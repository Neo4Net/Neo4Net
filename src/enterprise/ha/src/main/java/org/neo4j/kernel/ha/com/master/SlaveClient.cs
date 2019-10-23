using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha.com.master
{
	using InstanceId = Neo4Net.cluster.InstanceId;
	using Neo4Net.com;
	using Neo4Net.com;
	using Protocol = Neo4Net.com.Protocol;
	using ProtocolVersion = Neo4Net.com.ProtocolVersion;
	using RequestContext = Neo4Net.com.RequestContext;
	using RequestType = Neo4Net.com.RequestType;
	using Neo4Net.com;
	using Neo4Net.com;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using SlaveServer = Neo4Net.Kernel.ha.com.slave.SlaveServer;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.VOID_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.readString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.writeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.storecopy.ResponseUnpacker_Fields.NO_OP_RESPONSE_UNPACKER;

	public class SlaveClient : Client<Slave>, Slave
	{
		 private readonly InstanceId _machineId;

		 public SlaveClient( InstanceId machineId, string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, int maxConcurrentChannels, int chunkSize, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader ) : base( destinationHostNameOrIp, destinationPort, originHostNameOrIp, logProvider, storeId, Protocol.DEFAULT_FRAME_LENGTH, HaSettings.read_timeout.apply( from -> null ).toMillis(), maxConcurrentChannels, chunkSize, NO_OP_RESPONSE_UNPACKER, byteCounterMonitor, requestMonitor, entryReader )
		 {
			  this._machineId = machineId;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.com.Response<Void> pullUpdates(final long upToAndIncludingTxId)
		 public override Response<Void> PullUpdates( long upToAndIncludingTxId )
		 {
			  return SendRequest(SlaveRequestType.PullUpdates, RequestContext.EMPTY, buffer =>
			  {
				writeString( buffer, NeoStoreDataSource.DEFAULT_DATA_SOURCE_NAME );
				buffer.writeLong( upToAndIncludingTxId );
			  }, Protocol.VOID_DESERIALIZER);
		 }

		 public override ProtocolVersion ProtocolVersion
		 {
			 get
			 {
				  return SlaveServer.SLAVE_PROTOCOL_VERSION;
			 }
		 }

		 public sealed class SlaveRequestType : RequestType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           PULL_UPDATES((org.Neo4Net.com.TargetCaller<Slave, Void>)(master, context, input, target) ->{readString(input);

			  private static readonly IList<SlaveRequestType> valueList = new List<SlaveRequestType>();

			  static SlaveRequestType()
			  {
				  valueList.Add( PULL_UPDATES );
			  }

			  public enum InnerEnum
			  {
				  PULL_UPDATES
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private SlaveRequestType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal readonly Neo4Net.com.TargetCaller caller;
			  internal readonly Neo4Net.com.ObjectSerializer serializer;

			  internal SlaveRequestType( string name, InnerEnum innerEnum, Neo4Net.com.TargetCaller caller, Neo4Net.com.ObjectSerializer serializer )
			  {
					this._caller = caller;
					this._serializer = serializer;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public Neo4Net.com.TargetCaller TargetCaller
			  {
				  get
				  {
						return _caller;
				  }
			  }

			  public Neo4Net.com.ObjectSerializer ObjectSerializer
			  {
				  get
				  {
						return _serializer;
				  }
			  }

			  public sbyte Id()
			  {
					return ( sbyte ) ordinal();
			  }

			  public bool ResponseShouldBeUnpacked()
			  {
					return false;
			  }

			 public static IList<SlaveRequestType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static SlaveRequestType ValueOf( string name )
			 {
				 foreach ( SlaveRequestType enumInstance in SlaveRequestType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 public virtual int ServerId
		 {
			 get
			 {
				  return _machineId.toIntegerIndex();
			 }
		 }
	}

}
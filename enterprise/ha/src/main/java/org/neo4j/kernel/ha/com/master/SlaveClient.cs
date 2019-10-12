using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.ha.com.master
{
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Protocol = Org.Neo4j.com.Protocol;
	using ProtocolVersion = Org.Neo4j.com.ProtocolVersion;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using RequestType = Org.Neo4j.com.RequestType;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using SlaveServer = Org.Neo4j.Kernel.ha.com.slave.SlaveServer;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.VOID_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.readString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.writeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.ResponseUnpacker_Fields.NO_OP_RESPONSE_UNPACKER;

	public class SlaveClient : Client<Slave>, Slave
	{
		 private readonly InstanceId _machineId;

		 public SlaveClient( InstanceId machineId, string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, int maxConcurrentChannels, int chunkSize, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader ) : base( destinationHostNameOrIp, destinationPort, originHostNameOrIp, logProvider, storeId, Protocol.DEFAULT_FRAME_LENGTH, HaSettings.read_timeout.apply( from -> null ).toMillis(), maxConcurrentChannels, chunkSize, NO_OP_RESPONSE_UNPACKER, byteCounterMonitor, requestMonitor, entryReader )
		 {
			  this._machineId = machineId;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.com.Response<Void> pullUpdates(final long upToAndIncludingTxId)
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
//           PULL_UPDATES((org.neo4j.com.TargetCaller<Slave, Void>)(master, context, input, target) ->{readString(input);

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

			  internal readonly Org.Neo4j.com.TargetCaller caller;
			  internal readonly Org.Neo4j.com.ObjectSerializer serializer;

			  internal SlaveRequestType( string name, InnerEnum innerEnum, Org.Neo4j.com.TargetCaller caller, Org.Neo4j.com.ObjectSerializer serializer )
			  {
					this._caller = caller;
					this._serializer = serializer;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public Org.Neo4j.com.TargetCaller TargetCaller
			  {
				  get
				  {
						return _caller;
				  }
			  }

			  public Org.Neo4j.com.ObjectSerializer ObjectSerializer
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

			 public static SlaveRequestType valueOf( string name )
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
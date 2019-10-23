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
namespace Neo4Net.backup.impl
{

	using Neo4Net.com;
	using Neo4Net.com;
	using Protocol = Neo4Net.com.Protocol;
	using ProtocolVersion = Neo4Net.com.ProtocolVersion;
	using RequestContext = Neo4Net.com.RequestContext;
	using RequestType = Neo4Net.com.RequestType;
	using Neo4Net.com;
	using Neo4Net.com;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using ToNetworkStoreWriter = Neo4Net.com.storecopy.ToNetworkStoreWriter;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.backup.impl.BackupServer.BACKUP_PROTOCOL_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.backup.impl.BackupServer.FRAME_LENGTH;

	public class BackupClient : Client<TheBackupInterface>, TheBackupInterface
	{

		 public static readonly long BigReadTimeout = TimeUnit.MINUTES.toMillis( 20 );

		 internal BackupClient( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, long timeout, ResponseUnpacker unpacker, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> reader ) : base( destinationHostNameOrIp, destinationPort, originHostNameOrIp, logProvider, storeId, FRAME_LENGTH, timeout, Client.DEFAULT_MAX_NUMBER_OF_CONCURRENT_CHANNELS_PER_CLIENT, FRAME_LENGTH, unpacker, byteCounterMonitor, requestMonitor, reader )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.com.Response<Void> fullBackup(org.Neo4Net.com.storecopy.StoreWriter storeWriter, final boolean forensics)
		 public override Response<Void> FullBackup( StoreWriter storeWriter, bool forensics )
		 {
			  return SendRequest( BackupRequestType.FullBackup, RequestContext.EMPTY, buffer => buffer.writeByte( forensics ? ( sbyte ) 1 : ( sbyte ) 0 ), new Protocol.FileStreamsDeserializer310( storeWriter ) );
		 }

		 public override Response<Void> IncrementalBackup( RequestContext context )
		 {
			  return SendRequest( BackupRequestType.IncrementalBackup, context, Protocol.EMPTY_SERIALIZER, Protocol.VOID_DESERIALIZER );
		 }

		 public override ProtocolVersion ProtocolVersion
		 {
			 get
			 {
				  return BACKUP_PROTOCOL_VERSION;
			 }
		 }

		 protected internal override bool ShouldCheckStoreId( RequestType type )
		 {
			  return type != BackupRequestType.FullBackup;
		 }

		 public sealed class BackupRequestType : RequestType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FULL_BACKUP((org.Neo4Net.com.TargetCaller<org.Neo4Net.backup.TheBackupInterface, Void>)(master, context, input, target) ->{boolean forensics = input.readable() && booleanOf(input.readByte());return master.fullBackup(new org.Neo4Net.com.storecopy.ToNetworkStoreWriter(target, new org.Neo4Net.kernel.monitoring.Monitors()), forensics);}, org.Neo4Net.com.Protocol.VOID_SERIALIZER),
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INCREMENTAL_BACKUP((org.Neo4Net.com.TargetCaller<org.Neo4Net.backup.TheBackupInterface, Void>)(master, context, input, target) -> master.incrementalBackup(context), org.Neo4Net.com.Protocol.VOID_SERIALIZER);

			  private static readonly IList<BackupRequestType> valueList = new List<BackupRequestType>();

			  static BackupRequestType()
			  {
				  valueList.Add( FULL_BACKUP );
				  valueList.Add( INCREMENTAL_BACKUP );
			  }

			  public enum InnerEnum
			  {
				  FULL_BACKUP,
				  INCREMENTAL_BACKUP
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private BackupRequestType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal readonly Neo4Net.com.TargetCaller<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard> masterCaller;
			  internal readonly Neo4Net.com.ObjectSerializer<JavaToDotNetGenericWildcard> serializer;

			  internal BackupRequestType<T1, T2>( string name, InnerEnum innerEnum, Neo4Net.com.TargetCaller<T1> masterCaller, Neo4Net.com.ObjectSerializer<T2> serializer )
			  {
					this._masterCaller = masterCaller;
					this._serializer = serializer;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static bool BooleanOf( sbyte value )
			  {
					switch ( value )
					{
					case 0:
						return false;
					case 1:
						return true;
					default:
						throw new System.ArgumentException( "Invalid 'boolean' byte value " + value );
					}
			  }

			  public Neo4Net.com.TargetCaller<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard> TargetCaller
			  {
				  get
				  {
						return _masterCaller;
				  }
			  }

			  public Neo4Net.com.ObjectSerializer<JavaToDotNetGenericWildcard> ObjectSerializer
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

			 public static IList<BackupRequestType> values()
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

			 public static BackupRequestType ValueOf( string name )
			 {
				 foreach ( BackupRequestType enumInstance in BackupRequestType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}
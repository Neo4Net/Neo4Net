using System;
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
namespace Org.Neo4j.com
{
	using Channel = org.jboss.netty.channel.Channel;

	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Client.DEFAULT_READ_RESPONSE_TIMEOUT_SECONDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.readString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class MadeUpServer : Server<MadeUpCommunicationInterface, Void>
	{
		 private volatile bool _responseWritten;
		 private volatile bool _responseFailureEncountered;
		 private readonly sbyte _internalProtocolVersion;
		 internal const int FRAME_LENGTH = 1024 * 1024;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: MadeUpServer(MadeUpCommunicationInterface requestTarget, final int port, byte internalProtocolVersion, byte applicationProtocolVersion, TxChecksumVerifier txVerifier, final int chunkSize)
		 internal MadeUpServer( MadeUpCommunicationInterface requestTarget, int port, sbyte internalProtocolVersion, sbyte applicationProtocolVersion, TxChecksumVerifier txVerifier, int chunkSize ) : base(requestTarget, new ConfigurationAnonymousInnerClass(port, chunkSize)
		 {
						, Instance, FRAME_LENGTH, new ProtocolVersion( applicationProtocolVersion, ProtocolVersion.INTERNAL_PROTOCOL_VERSION ), txVerifier, Clocks.systemClock(), (new Monitors()).newMonitor(typeof(ByteCounterMonitor)), (new Monitors()).newMonitor(typeof(RequestMonitor)));
			  this._internalProtocolVersion = internalProtocolVersion;
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private int _port;
			 private int _chunkSize;

			 public ConfigurationAnonymousInnerClass( int port, int chunkSize )
			 {
				 this._port = port;
				 this._chunkSize = chunkSize;
			 }

			 public long OldChannelThreshold
			 {
				 get
				 {
					  return DEFAULT_READ_RESPONSE_TIMEOUT_SECONDS * 1000;
				 }
			 }

			 public int MaxConcurrentTransactions
			 {
				 get
				 {
					  return DEFAULT_MAX_NUMBER_OF_CONCURRENT_TRANSACTIONS;
				 }
			 }

			 public int ChunkSize
			 {
				 get
				 {
					  return _chunkSize;
				 }
			 }

			 public HostnamePort ServerAddress
			 {
				 get
				 {
					  return new HostnamePort( null, _port );
				 }
			 }
		 }

		 protected internal override void ResponseWritten( RequestType type, Channel channel, RequestContext context )
		 {
			  _responseWritten = true;
		 }

		 protected internal override void WriteFailureResponse( Exception exception, ChunkingChannelBuffer buffer )
		 {
			  _responseFailureEncountered = true;
			  base.WriteFailureResponse( exception, buffer );
		 }

		 protected internal override sbyte InternalProtocolVersion
		 {
			 get
			 {
				  return _internalProtocolVersion;
			 }
		 }

		 protected internal override RequestType GetRequestContext( sbyte id )
		 {
			  return MadeUpRequestType.values()[id];
		 }

		 protected internal override void StopConversation( RequestContext context )
		 {
		 }

		 internal virtual bool ResponseHasBeenWritten()
		 {
			  return _responseWritten;
		 }

		 internal virtual bool ResponseFailureEncountered()
		 {
			  return _responseFailureEncountered;
		 }

		 internal sealed class MadeUpRequestType : RequestType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           MULTIPLY((master, context, input, target) ->{int value1 = input.readInt();int value2 = input.readInt();return master.multiply(value1, value2);}, Protocol.INTEGER_SERIALIZER, 0),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FETCH_DATA_STREAM((master, context, input, target) ->{int dataSize = input.readInt();return master.fetchDataStream(new ToChannelBufferWriter(target), dataSize);}, Protocol.VOID_SERIALIZER),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SEND_DATA_STREAM((master, context, input, target) ->{try(BlockLogReader reader = new BlockLogReader(input)){return master.sendDataStream(reader);}}, Protocol.VOID_SERIALIZER){ boolean responseShouldBeUnpacked(){return false;}},

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           THROW_EXCEPTION((master, context, input, target) -> master.throwException(readString(input)), Protocol.VOID_SERIALIZER, 0),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CAUSE_READ_CONTEXT_EXCEPTION((master, context, input, target) ->{throw new AssertionError("Test should not reach this far, it should fail while reading the request context.");}, Protocol.VOID_SERIALIZER),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           STREAM_BACK_TRANSACTIONS((master, context, input, target) -> master.streamBackTransactions(input.readInt(), input.readInt()), Protocol.INTEGER_SERIALIZER, 0),

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INFORM_ABOUT_TX_OBLIGATIONS((master, context, input, target) -> master.informAboutTransactionObligations(input.readInt(), input.readLong()), Protocol.INTEGER_SERIALIZER, 0);

			  private static readonly IList<MadeUpRequestType> valueList = new List<MadeUpRequestType>();

			  static MadeUpRequestType()
			  {
				  valueList.Add( MULTIPLY );
				  valueList.Add( FETCH_DATA_STREAM );
				  valueList.Add( SEND_DATA_STREAM );
				  valueList.Add( THROW_EXCEPTION );
				  valueList.Add( CAUSE_READ_CONTEXT_EXCEPTION );
				  valueList.Add( STREAM_BACK_TRANSACTIONS );
				  valueList.Add( INFORM_ABOUT_TX_OBLIGATIONS );
			  }

			  public enum InnerEnum
			  {
				  MULTIPLY,
				  FETCH_DATA_STREAM,
				  SEND_DATA_STREAM,
				  THROW_EXCEPTION,
				  CAUSE_READ_CONTEXT_EXCEPTION,
				  STREAM_BACK_TRANSACTIONS,
				  INFORM_ABOUT_TX_OBLIGATIONS
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private MadeUpRequestType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal readonly TargetCaller masterCaller;
			  internal readonly ObjectSerializer serializer;

			  internal MadeUpRequestType( string name, InnerEnum innerEnum, TargetCaller<MadeUpCommunicationInterface, int> masterCaller, ObjectSerializer serializer, int ignore )
			  {
					this._masterCaller = masterCaller;
					this._serializer = serializer;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal MadeUpRequestType( string name, InnerEnum innerEnum, TargetCaller<MadeUpCommunicationInterface, Void> masterCaller, ObjectSerializer serializer )
			  {
					this._masterCaller = masterCaller;
					this._serializer = serializer;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public TargetCaller TargetCaller
			  {
				  get
				  {
						return this._masterCaller;
				  }
			  }

			  public ObjectSerializer ObjectSerializer
			  {
				  get
				  {
						return this._serializer;
				  }
			  }

			  public sbyte Id()
			  {
					return ( sbyte ) ordinal();
			  }

			  public bool ResponseShouldBeUnpacked()
			  {
					return true;
			  }

			 public static IList<MadeUpRequestType> values()
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

			 public static MadeUpRequestType valueOf( string name )
			 {
				 foreach ( MadeUpRequestType enumInstance in MadeUpRequestType.valueList )
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
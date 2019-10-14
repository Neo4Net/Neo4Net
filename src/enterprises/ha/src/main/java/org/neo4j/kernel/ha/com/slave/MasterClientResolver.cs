using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.com.slave
{

	using ComException = Neo4Net.com.ComException;
	using ComExceptionHandler = Neo4Net.com.ComExceptionHandler;
	using IllegalProtocolVersionException = Neo4Net.com.IllegalProtocolVersionException;
	using ProtocolVersion = Neo4Net.com.ProtocolVersion;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using InvalidEpochException = Neo4Net.Kernel.ha.com.master.InvalidEpochException;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	public class MasterClientResolver : MasterClientFactory, ComExceptionHandler
	{
		 private volatile MasterClientFactory _currentFactory;

		 private readonly IDictionary<ProtocolVersion, MasterClientFactory> _protocolToFactoryMapping;
		 private readonly Log _log;
		 private readonly ResponseUnpacker _responseUnpacker;
		 private readonly InvalidEpochExceptionHandler _invalidEpochHandler;
		 private readonly System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> _logEntryReader;

		 public override MasterClient Instantiate( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, Monitors monitors, StoreId storeId, LifeSupport life )
		 {
			  if ( _currentFactory == null )
			  {
					AssignDefaultFactory();
			  }

			  MasterClient result = _currentFactory.instantiate( destinationHostNameOrIp, destinationPort, originHostNameOrIp, monitors, storeId, life );
			  result.ComExceptionHandler = this;
			  return result;
		 }

		 public MasterClientResolver( LogProvider logProvider, ResponseUnpacker responseUnpacker, InvalidEpochExceptionHandler invalidEpochHandler, int readTimeoutMillis, int lockReadTimeout, int channels, int chunkSize, System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> logEntryReader )
		 {
			  this._logEntryReader = logEntryReader;
			  this._log = logProvider.getLog( this.GetType() );
			  this._responseUnpacker = responseUnpacker;
			  this._invalidEpochHandler = invalidEpochHandler;

			  _protocolToFactoryMapping = new Dictionary<ProtocolVersion, MasterClientFactory>( 3, 1 );
			  _protocolToFactoryMapping[MasterClient214.PROTOCOL_VERSION] = new F214( this, logProvider, readTimeoutMillis, lockReadTimeout, channels, chunkSize );
			  _protocolToFactoryMapping[MasterClient310.PROTOCOL_VERSION] = new F310( this, logProvider, readTimeoutMillis, lockReadTimeout, channels, chunkSize );
			  _protocolToFactoryMapping[MasterClient320.PROTOCOL_VERSION] = new F320( this, logProvider, readTimeoutMillis, lockReadTimeout, channels, chunkSize );
		 }

		 public override void Handle( ComException exception )
		 {
			  exception.TraceComException( _log, "MasterClientResolver.handle" );
			  if ( exception is IllegalProtocolVersionException )
			  {
					_log.info( "Handling " + exception + ", will pick new master client" );

					IllegalProtocolVersionException illegalProtocolVersion = ( IllegalProtocolVersionException ) exception;
					ProtocolVersion requiredProtocolVersion = new ProtocolVersion( illegalProtocolVersion.Received, ProtocolVersion.INTERNAL_PROTOCOL_VERSION );
					GetFor( requiredProtocolVersion );
			  }
			  else if ( exception is InvalidEpochException )
			  {
					_log.info( "Handling " + exception + ", will go to PENDING and ask for election" );

					_invalidEpochHandler.handle();
			  }
			  else
			  {
					_log.debug( "Ignoring " + exception + "." );
			  }
		 }

		 private MasterClientFactory GetFor( ProtocolVersion protocolVersion )
		 {
			  MasterClientFactory candidate = _protocolToFactoryMapping[protocolVersion];
			  if ( candidate != null )
			  {
					_currentFactory = candidate;
			  }
			  return candidate;
		 }

		 private MasterClientFactory AssignDefaultFactory()
		 {
			  return GetFor( MasterClient320.PROTOCOL_VERSION );
		 }

		 private abstract class StaticMasterClientFactory : MasterClientFactory
		 {
			 public abstract MasterClient Instantiate( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, Monitors monitors, StoreId storeId, LifeSupport life );
			  internal readonly LogProvider LogProvider;
			  internal readonly int ReadTimeoutMillis;
			  internal readonly int LockReadTimeout;
			  internal readonly int MaxConcurrentChannels;
			  internal readonly int ChunkSize;

			  internal StaticMasterClientFactory( LogProvider logProvider, int readTimeoutMillis, int lockReadTimeout, int maxConcurrentChannels, int chunkSize )
			  {
					this.LogProvider = logProvider;
					this.ReadTimeoutMillis = readTimeoutMillis;
					this.LockReadTimeout = lockReadTimeout;
					this.MaxConcurrentChannels = maxConcurrentChannels;
					this.ChunkSize = chunkSize;
			  }
		 }

		 private sealed class F214 : StaticMasterClientFactory
		 {
			 private readonly MasterClientResolver _outerInstance;

			  internal F214( MasterClientResolver outerInstance, LogProvider logProvider, int readTimeoutMillis, int lockReadTimeout, int maxConcurrentChannels, int chunkSize ) : base( logProvider, readTimeoutMillis, lockReadTimeout, maxConcurrentChannels, chunkSize )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override MasterClient Instantiate( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, Monitors monitors, StoreId storeId, LifeSupport life )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return life.Add( new MasterClient214( destinationHostNameOrIp, destinationPort, originHostNameOrIp, LogProvider, storeId, ReadTimeoutMillis, LockReadTimeout, MaxConcurrentChannels, ChunkSize, outerInstance.responseUnpacker, monitors.NewMonitor( typeof( ByteCounterMonitor ), typeof( MasterClient320 ).FullName ), monitors.NewMonitor( typeof( RequestMonitor ), typeof( MasterClient320 ).FullName ), outerInstance.logEntryReader.get() ) );
			  }
		 }

		 private sealed class F310 : StaticMasterClientFactory
		 {
			 private readonly MasterClientResolver _outerInstance;

			  internal F310( MasterClientResolver outerInstance, LogProvider logProvider, int readTimeoutMillis, int lockReadTimeout, int maxConcurrentChannels, int chunkSize ) : base( logProvider, readTimeoutMillis, lockReadTimeout, maxConcurrentChannels, chunkSize )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override MasterClient Instantiate( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, Monitors monitors, StoreId storeId, LifeSupport life )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return life.Add( new MasterClient310( destinationHostNameOrIp, destinationPort, originHostNameOrIp, LogProvider, storeId, ReadTimeoutMillis, LockReadTimeout, MaxConcurrentChannels, ChunkSize, outerInstance.responseUnpacker, monitors.NewMonitor( typeof( ByteCounterMonitor ), typeof( MasterClient320 ).FullName ), monitors.NewMonitor( typeof( RequestMonitor ), typeof( MasterClient320 ).FullName ), outerInstance.logEntryReader.get() ) );
			  }
		 }

		 private sealed class F320 : StaticMasterClientFactory
		 {
			 private readonly MasterClientResolver _outerInstance;

			  internal F320( MasterClientResolver outerInstance, LogProvider logProvider, int readTimeoutMillis, int lockReadTimeout, int maxConcurrentChannels, int chunkSize ) : base( logProvider, readTimeoutMillis, lockReadTimeout, maxConcurrentChannels, chunkSize )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override MasterClient Instantiate( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, Monitors monitors, StoreId storeId, LifeSupport life )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return life.Add( new MasterClient320( destinationHostNameOrIp, destinationPort, originHostNameOrIp, LogProvider, storeId, ReadTimeoutMillis, LockReadTimeout, MaxConcurrentChannels, ChunkSize, outerInstance.responseUnpacker, monitors.NewMonitor( typeof( ByteCounterMonitor ), typeof( MasterClient320 ).FullName ), monitors.NewMonitor( typeof( RequestMonitor ), typeof( MasterClient320 ).FullName ), outerInstance.logEntryReader.get() ) );
			  }
		 }
	}

}
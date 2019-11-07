using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

	using RecordStorageCommandReaderFactory = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using CommandReaderFactory = Neo4Net.Kernel.Api.StorageEngine.CommandReaderFactory;
	using ReadPastEndException = Neo4Net.Kernel.Api.StorageEngine.ReadPastEndException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.throwIfInstanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.withMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.entry.LogEntrySanity.logEntryMakesSense;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.entry.LogEntryVersion.byVersion;

	/// <summary>
	/// Version aware implementation of LogEntryReader
	/// Starting with Neo4Net version 2.1, log entries are prefixed with a version. This allows for Neo4Net instances of
	/// different versions to exchange transaction data, either directly or via logical logs.
	/// 
	/// Read all about it at <seealso cref="LogEntryVersion"/>.
	/// </summary>
	public class VersionAwareLogEntryReader<SOURCE> : LogEntryReader<SOURCE> where SOURCE : Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel
	{
		 private readonly CommandReaderFactory _commandReaderFactory;
		 private readonly InvalidLogEntryHandler _invalidLogEntryHandler;

		 public VersionAwareLogEntryReader() : this(new RecordStorageCommandReaderFactory(), InvalidLogEntryHandler.STRICT)
		 {
		 }

		 public VersionAwareLogEntryReader( CommandReaderFactory commandReaderFactory, InvalidLogEntryHandler invalidLogEntryHandler )
		 {
			  this._commandReaderFactory = commandReaderFactory;
			  this._invalidLogEntryHandler = invalidLogEntryHandler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogEntry readLogEntry(SOURCE channel) throws java.io.IOException
		 public override LogEntry ReadLogEntry( SOURCE channel )
		 {
			  try
			  {
					LogPositionMarker positionMarker = new LogPositionMarker();
					long skipped = 0;
					while ( true )
					{
						 channel.GetCurrentPosition( positionMarker );

						 sbyte versionCode = channel.Get();
						 sbyte typeCode = channel.Get();

						 LogEntryVersion version = null;
						 LogEntryParser<LogEntry> entryReader;
						 LogEntry entry;
						 try
						 {
							  version = byVersion( versionCode );
							  entryReader = version.entryParser( typeCode );
							  entry = entryReader.Parse( version, channel, positionMarker, _commandReaderFactory );
							  if ( entry != null && skipped > 0 )
							  {
									// Take extra care when reading an entry in a bad section. Just because entry reading
									// didn't throw exception doesn't mean that it's a sane entry.
									if ( !logEntryMakesSense( entry ) )
									{
										 throw new System.ArgumentException( "Log entry " + entry + " which was read after " + "a bad section of " + skipped + " bytes was read successfully, but " + "its contents is unrealistic, so treating as part of bad section" );
									}
									_invalidLogEntryHandler.bytesSkipped( skipped );
									skipped = 0;
							  }
						 }
						 catch ( ReadPastEndException e )
						 { // Make these exceptions slip by straight out to the outer handler
							  throw e;
						 }
						 catch ( Exception e )
						 { // Tag all other exceptions with log position and other useful information
							  LogPosition position = positionMarker.NewPosition();
							  e = withMessage( e, e.Message + ". At position " + position + " and entry version " + version );

							  if ( ChannelSupportsPositioning( channel ) && _invalidLogEntryHandler.handleInvalidEntry( e, position ) )
							  {
									( ( PositionableChannel )channel ).CurrentPosition = positionMarker.ByteOffset + 1;
									skipped++;
									continue;
							  }
							  throwIfInstanceOf( e, typeof( UnsupportedLogVersionException ) );
							  throw new IOException( e );
						 }

						 if ( !entryReader.Skip() )
						 {
							  return entry;
						 }
					}
			  }
			  catch ( ReadPastEndException )
			  {
					return null;
			  }
		 }

		 private bool ChannelSupportsPositioning( SOURCE channel )
		 {
			  return channel is PositionableChannel;
		 }
	}

}
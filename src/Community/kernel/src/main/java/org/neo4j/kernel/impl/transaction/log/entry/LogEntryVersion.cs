using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
	using PhysicalLogCommandReaderV3_0_2 = Neo4Net.Kernel.impl.transaction.command.PhysicalLogCommandReaderV3_0_2;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	/// <summary>
	/// Main entry point into log entry versions and parsers and all that. A <seealso cref="LogEntryVersion"/> can be retrieved
	/// by using <seealso cref="byVersion(sbyte)"/> and from there get a hold of
	/// <seealso cref="LogEntryParser"/> using <seealso cref="entryParser(sbyte)"/>.
	/// 
	/// Here follows an explanation how log entry versioning in Neo4j works:
	/// 
	/// In Neo4j transactions are written to a log. Each transaction consists of one or more log entries. Log entries
	/// can be of one or more types, such as denoting start of a transaction, commands and committing the transaction.
	/// Neo4j supports writing the latest/current log entry and reading log entries for all currently supported versions
	/// of Neo4j. The way versioning is done has changed over the years.
	///   First there was a format header of the entire log and it was assumed that all log entries within that log
	/// was of the same format. This version actually specified command version, i.e. just versions of one of the
	/// log entry types. This was a bit clunky and forced format specification to be passed in from outside,
	/// based on the log that was read and so updated every time a new log was opened.
	///   Starting with Neo4j version 2.1 a one-byte log version field was introduced with every single log entry.
	/// This allowed for more flexible reading and simpler code. Versions started with negative number to be able to
	/// distinguish the new format from the non-versioned format. So observing the log entry type, which was the first
	/// byte in each log entry being negative being negative was a signal for the new format and that the type actually
	/// was the next byte. This to support rolling upgrades where two Neo4j versions in a cluster could be active
	/// simultaneously, yet talking in terms of log entries of different versions.
	/// 
	/// At this point in time there was the log entry version which signaled how an entry was to be read, but there
	/// was still the log-global format version which didn't really do anything but make things complicated in the code.
	///   As of 2.2.4 the log-global format version is gone, although still just a token value written to adhere to
	/// the 16 bytes header size of a log for backwards compatibility. The log entry version controls everything
	/// about versioning of log entries and commands, such that if either log entry format (such as log entry types,
	/// such as START, COMMIT and the likes, or data within them) change, or one or more command format change
	/// the log entry version will be bumped.
	/// The process of making an update to log entry or command format is to:
	/// <ol>
	/// <li>Copy <seealso cref="PhysicalLogCommandReaderV3_0_2"/> or similar and modify the new copy</li>
	/// <li>Copy <seealso cref="LogEntryParsersV2_3"/> or similar and modify the new copy if entry layout has changed</li>
	/// <li>Add an entry in this enum, like <seealso cref="V3_0_10"/> pointing to the above new classes, version needs to be negative
	/// to detect log files from older versions of neo4j</li>
	/// <li>Modify <seealso cref="StorageCommand.serialize(WritableChannel)"/>.
	/// Also <seealso cref="LogEntryWriter"/> (if log entry layout has changed) with required changes</li>
	/// <li>Change <seealso cref="CURRENT"/> to point to the newly created version</li>
	/// </ol>
	/// Everything apart from that should just work and Neo4j should automatically support the new version as well.
	/// </summary>
	public sealed class LogEntryVersion
	{
		 public static readonly LogEntryVersion V2_3 = new LogEntryVersion( "V2_3", InnerEnum.V2_3, -5, typeof( LogEntryParsersV2_3 ) );
		 public static readonly LogEntryVersion V3_0 = new LogEntryVersion( "V3_0", InnerEnum.V3_0, -6, typeof( LogEntryParsersV2_3 ) );
		 // as of 2016-05-30: neo4j 2.3.5 explicit index IndexDefineCommand maps write size as short instead of byte
		 // log entry layout hasn't changed since 2_3 so just use that one
		 public static readonly LogEntryVersion V2_3_5 = new LogEntryVersion( "V2_3_5", InnerEnum.V2_3_5, -8, typeof( LogEntryParsersV2_3 ) );
		 // as of 2016-05-30: neo4j 3.0.2 explicit index IndexDefineCommand maps write size as short instead of byte
		 // log entry layout hasn't changed since 2_3 so just use that one
		 public static readonly LogEntryVersion V3_0_2 = new LogEntryVersion( "V3_0_2", InnerEnum.V3_0_2, -9, typeof( LogEntryParsersV2_3 ) );
		 // as of 2017-05-26: the records in command log entries include a bit that specifies if the command is serialised
		 // using a fixed-width reference format, or not. This change is technically backwards compatible, so we bump the
		 // log version to prevent mixed-version clusters from forming.
		 public static readonly LogEntryVersion V3_0_10 = new LogEntryVersion( "V3_0_10", InnerEnum.V3_0_10, -10, typeof( LogEntryParsersV2_3 ) );

		 private static readonly IList<LogEntryVersion> valueList = new List<LogEntryVersion>();

		 public enum InnerEnum
		 {
			 V2_3,
			 V3_0,
			 V2_3_5,
			 V3_0_2,
			 V3_0_10
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 // Method moreRecentVersionExists() relies on the fact that we have negative numbers, thus next version to use is -11

		 internal Public const;
		 internal Private const;
		 internal Private const;
		 internal Private static;
		 static LogEntryVersion()
		 {
			  _lookupByVersion = new LogEntryVersion[( -Current.byteCode() ) + 1]; // pessimistic size
			  foreach ( LogEntryVersion version in _all )
			  {
					Put( _lookupByVersion, -version.ByteCode(), version );
			  }

			 valueList.Add( V2_3 );
			 valueList.Add( V3_0 );
			 valueList.Add( V2_3_5 );
			 valueList.Add( V3_0_2 );
			 valueList.Add( V3_0_10 );
		 }

		 internal Private readonly;
		 internal Private readonly;

		 internal LogEntryVersion( string name, InnerEnum innerEnum, int version, Type cls )
		 {
			  this._entryTypes = new LogEntryParser[HighestCode( cls ) + 1];
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Enum<? extends LogEntryParser<? extends LogEntry>> parser : cls.getEnumConstants())
			  foreach ( Enum<LogEntryParser<LogEntry>> parser in cls.EnumConstants )
			  {
					LogEntryParser<LogEntry> candidate = ( LogEntryParser<LogEntry> ) parser;
					this._entryTypes[candidate.ByteCode()] = candidate;
			  }
			  this._version = SafeCastToByte( version );

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <returns> byte value representing this log entry version. </returns>
		 public sbyte ByteCode()
		 {
			  return _version;
		 }

		 /// <param name="type"> type of entry. </param>
		 /// <returns> a <seealso cref="LogEntryParser"/> capable of reading a <seealso cref="LogEntry"/> of the given type for this
		 /// log entry version. </returns>
		 public LogEntryParser<LogEntry> EntryParser( sbyte type )
		 {
			  LogEntryParser<LogEntry> candidate = ( type >= 0 && type < _entryTypes.Length ) ? _entryTypes[type] : null;
			  if ( candidate == null )
			  {
					throw new System.ArgumentException( "Unknown entry type " + type + " for version " + _version );
			  }
			  return candidate;
		 }

		 /// <summary>
		 /// Check if a more recent version of the log entry format exists and can be handled.
		 /// </summary>
		 /// <param name="version"> to compare against latest version </param>
		 /// <returns> {@code true} if a more recent log entry version exists </returns>
		 public static bool MoreRecentVersionExists( LogEntryVersion version )
		 {
			  return version._version > Current.version; // reverted do to negative version numbers
		 }

		 /// <summary>
		 /// Return the correct <seealso cref="LogEntryVersion"/> for the given {@code version} code read from e.g. a log entry.
		 /// Lookup is fast and can be made inside critical paths, no need for externally caching the returned
		 /// <seealso cref="LogEntryVersion"/> instance per the input arguments.
		 /// </summary>
		 /// <param name="version"> log entry version </param>
		 public static LogEntryVersion ByVersion( sbyte version )
		 {
			  sbyte positiveVersion = ( sbyte ) - version;

			  if ( positiveVersion >= _lowestVersion && positiveVersion < _lookupByVersion.Length )
			  {
					return _lookupByVersion[positiveVersion];
			  }
			  sbyte positiveCurrentVersion = ( sbyte ) - Current.byteCode();
			  if ( positiveVersion > positiveCurrentVersion )
			  {
					throw new UnsupportedLogVersionException( string.Format( "Transaction logs contains entries with prefix {0:D}, and the highest supported prefix is {1:D}. This " + "indicates that the log files originates from a newer version of neo4j.", positiveVersion, positiveCurrentVersion ) );
			  }
			  throw new UnsupportedLogVersionException( string.Format( "Transaction logs contains entries with prefix {0:D}, and the lowest supported prefix is {1:D}. This " + "indicates that the log files originates from an older version of neo4j, which we don't support " + "migrations from.", positiveVersion, _lowestVersion ) );
		 }

		 private static void Put( LogEntryVersion[] array, int index, LogEntryVersion version )
		 {
			  array[index] = version;
		 }

		 private static sbyte SafeCastToByte( int value )
		 {
			  bool reversed = false;
			  if ( value < 0 )
			  {
					value = ~value;
					reversed = true;
			  }

			  if ( ( value & ~0xFF ) != 0 )
			  {
					throw new Exception( format( "Bad version %d, must be contained within one byte", value ) );
			  }
			  return ( sbyte )( reversed ?~value : value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static int highestCode(Class cls)
		 private static int HighestCode( Type cls )
		 {
			  int highestCode = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Enum<? extends LogEntryParser<? extends LogEntry>> parser : cls.getEnumConstants())
			  foreach ( Enum<LogEntryParser<LogEntry>> parser in cls.EnumConstants )
			  {
					LogEntryParser<LogEntry> candidate = ( LogEntryParser<LogEntry> ) parser;
					highestCode = Math.Max( highestCode, candidate.ByteCode() );
			  }
			  return highestCode;
		 }

		public static IList<LogEntryVersion> values()
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

		public static LogEntryVersion valueOf( string name )
		{
			foreach ( LogEntryVersion enumInstance in LogEntryVersion.valueList )
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
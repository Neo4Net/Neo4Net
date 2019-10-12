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
namespace Org.Neo4j.Test.mockito.matcher
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using LogEntryCursor = Org.Neo4j.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using PhysicalLogVersionedStoreChannel = Org.Neo4j.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using ReadAheadLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableLogChannel;
	using CheckPoint = Org.Neo4j.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using LogHeader = Org.Neo4j.Kernel.impl.transaction.log.entry.LogHeader;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using Org.Neo4j.Kernel.impl.util;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;

	/// <summary>
	/// A set of hamcrest matchers for asserting logical logs look in certain ways.
	/// Please expand as necessary.
	/// </summary>
	public class LogMatchers
	{

		 private LogMatchers()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.List<org.neo4j.kernel.impl.transaction.log.entry.LogEntry> logEntries(org.neo4j.io.fs.FileSystemAbstraction fileSystem, String logPath) throws java.io.IOException
		 public static IList<LogEntry> LogEntries( FileSystemAbstraction fileSystem, string logPath )
		 {
			  File logFile = new File( logPath );
			  StoreChannel fileChannel = fileSystem.Open( logFile, OpenMode.READ );

			  // Always a header
			  LogHeader header = readLogHeader( ByteBuffer.allocate( LOG_HEADER_SIZE ), fileChannel, true, logFile );

			  // Read all log entries
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( fileChannel, header.LogVersion, header.LogFormatVersion );
			  ReadableLogChannel logChannel = new ReadAheadLogChannel( versionedStoreChannel );
			  LogEntryCursor logEntryCursor = new LogEntryCursor( new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>(), logChannel );
			  return Iterables.asList( new IOCursorAsResourceIterable<>( logEntryCursor ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.List<org.neo4j.kernel.impl.transaction.log.entry.LogEntry> logEntries(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file) throws java.io.IOException
		 public static IList<LogEntry> LogEntries( FileSystemAbstraction fileSystem, File file )
		 {
			  return logEntries( fileSystem, file.Path );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<java.util.List<org.neo4j.kernel.impl.transaction.log.entry.LogEntry>> containsExactly(final org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry>... matchers)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<IList<LogEntry>> ContainsExactly( params Matcher<LogEntry>[] matchers )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( matchers );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<IList<LogEntry>>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.hamcrest.Matcher<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry>[] matchers;
			 private Matcher<LogEntry>[] _matchers;

			 public TypeSafeMatcherAnonymousInnerClass<T1>( Matcher<T1>[] matchers ) where T1 : Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry
			 {
				 this._matchers = matchers;
			 }

			 public override bool matchesSafely( IList<LogEntry> item )
			 {
				  IEnumerator<LogEntry> actualEntries = item.GetEnumerator();
				  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry> matcher : matchers)
						foreach ( Matcher<LogEntry> matcher in _matchers )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 if ( actualEntries.hasNext() )
							 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
								  LogEntry next = actualEntries.next();
								  if ( !matcher.matches( next ) )
								  {
										// Wrong!
										return false;
								  }
							 }
							 else
							 {
								  // Too few actual entries!
								  return false;
							 }
						}

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( actualEntries.hasNext() )
						{
							 // Too many actual entries!
							 return false;
						}

						// All good in the hood :)
						return true;
				  }
			 }

			 public override void describeTo( Description description )
			 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry> matcher : matchers)
				  foreach ( Matcher<LogEntry> matcher in _matchers )
				  {
						description.appendDescriptionOf( matcher ).appendText( ",\n" );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry> startEntry(final int masterId, final int localId)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<LogEntry> StartEntry( int masterId, int localId )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( masterId, localId );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<LogEntryStart>
		 {
			 private int _masterId;
			 private int _localId;

			 public TypeSafeMatcherAnonymousInnerClass2( int masterId, int localId )
			 {
				 this._masterId = masterId;
				 this._localId = localId;
			 }

			 public override bool matchesSafely( LogEntryStart entry )
			 {
				  return entry != null && entry.MasterId == _masterId && entry.LocalId == _localId;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Start[" + "xid=<Any Xid>,master=" + _masterId + ",me=" + _localId + ",time=<Any Date>]" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry> commitEntry(final long txId)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<LogEntry> CommitEntry( long txId )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass3( txId );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass3 : TypeSafeMatcher<LogEntryCommit>
		 {
			 private long _txId;

			 public TypeSafeMatcherAnonymousInnerClass3( long txId )
			 {
				 this._txId = txId;
			 }

			 public override bool matchesSafely( LogEntryCommit onePC )
			 {
				  return onePC != null && onePC.TxId == _txId;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( string.Format( "Commit[txId={0:D}, <Any Date>]", _txId ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry> checkPoint(final org.neo4j.kernel.impl.transaction.log.LogPosition position)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<LogEntry> CheckPoint( LogPosition position )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass4( position );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass4 : TypeSafeMatcher<CheckPoint>
		 {
			 private LogPosition _position;

			 public TypeSafeMatcherAnonymousInnerClass4( LogPosition position )
			 {
				 this._position = position;
			 }

			 public override bool matchesSafely( CheckPoint cp )
			 {
				  return cp != null && _position.Equals( cp.LogPosition );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( string.Format( "CheckPoint[position={0}]", _position.ToString() ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? extends org.neo4j.kernel.impl.transaction.log.entry.LogEntry> commandEntry(final long key, final Class commandClass)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<LogEntry> CommandEntry( long key, Type commandClass )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass5( key, commandClass );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass5 : TypeSafeMatcher<LogEntryCommand>
		 {
			 private long _key;
			 private Type _commandClass;

			 public TypeSafeMatcherAnonymousInnerClass5( long key, Type commandClass )
			 {
				 this._key = key;
				 this._commandClass = commandClass;
			 }

			 public override bool matchesSafely( LogEntryCommand commandEntry )
			 {
				  if ( commandEntry == null )
				  {
						return false;
				  }

				  Command command = ( Command ) commandEntry.Command;
				  return command.Key == _key && command.GetType().Equals(_commandClass);
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( string.Format( "Command[key={0:D}, cls={1}]", _key, _commandClass.Name ) );
			 }
		 }
	}

}
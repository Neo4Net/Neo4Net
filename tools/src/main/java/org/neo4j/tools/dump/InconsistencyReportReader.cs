using System;
using System.IO;

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
namespace Org.Neo4j.tools.dump
{

	using RecordType = Org.Neo4j.Consistency.RecordType;
	using Inconsistencies = Org.Neo4j.tools.dump.inconsistency.Inconsistencies;

	/// <summary>
	/// Reads CC inconsistency reports. Example of entry:
	/// <para>
	/// <pre>
	/// ERROR: The referenced relationship record is not in use.
	///     Node[3496089,used=true,rel=14833798,prop=13305361,labels=Inline(0x1000000006:[6]),light,secondaryUnitId=-1]
	///     Inconsistent with: Relationship[14833798,used=false,source=0,target=0,type=0,sPrev=0,sNext=0,tPrev=0,tNext=0,
	///     prop=0,secondaryUnitId=-1,!sFirst,!tFirst]
	/// </pre>
	/// </para>
	/// <para>
	/// Another example entry:
	/// </para>
	/// <para>
	/// <pre>
	/// ERROR: The first outgoing relationship is not the first in its chain.
	///     RelationshipGroup[12144403,type=9,out=2988709379,in=-1,loop=-1,prev=-1,next=40467195,used=true,owner=635306933,secondaryUnitId=-1]
	/// </pre>
	/// </para>
	/// </summary>
	public class InconsistencyReportReader
	{
		 private const string INCONSISTENT_WITH = "Inconsistent with: ";
		 private readonly Inconsistencies _inconsistencies;

		 public InconsistencyReportReader( Inconsistencies inconsistencies )
		 {
			  this._inconsistencies = inconsistencies;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(java.io.File file) throws java.io.IOException
		 public virtual void Read( File file )
		 {
			  using ( StreamReader reader = new StreamReader( file ) )
			  {
					Read( reader );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(java.io.BufferedReader bufferedReader) throws java.io.IOException
		 public virtual void Read( StreamReader bufferedReader )
		 {
			  string line = bufferedReader.ReadLine();
			  RecordType inconsistentRecordType;
			  RecordType inconsistentWithRecordType;
			  long inconsistentRecordId;
			  long inconsistentWithRecordId;

			  while ( !string.ReferenceEquals( line, null ) )
			  {
					if ( line.Contains( "ERROR" ) || line.Contains( "WARNING" ) )
					{
						 // The current line is the inconsistency description line.
						 // Get the inconsistent entity line:
						 line = bufferedReader.ReadLine();
						 if ( string.ReferenceEquals( line, null ) )
						 {
							  return; // Unexpected end of report.
						 }
						 line = line.Trim();
						 inconsistentRecordType = ToRecordType( EntityType( line ) );
						 inconsistentRecordId = Id( line );

						 // Then get the Inconsistent With line:
						 line = bufferedReader.ReadLine();
						 if ( string.ReferenceEquals( line, null ) || !line.Contains( INCONSISTENT_WITH ) )
						 {
							  // There's no Inconsistent With line, so we report what we have.
							  _inconsistencies.reportInconsistency( inconsistentRecordType, inconsistentRecordId );
							  // Leave the current line for the next iteration of the loop.
						 }
						 else
						 {
							  line = line.Substring( INCONSISTENT_WITH.Length ).Trim();
							  inconsistentWithRecordType = ToRecordType( EntityType( line ) );
							  inconsistentWithRecordId = Id( line );
							  _inconsistencies.reportInconsistency( inconsistentRecordType, inconsistentRecordId, inconsistentWithRecordType, inconsistentWithRecordId );
							  line = bufferedReader.ReadLine(); // Prepare a line for the next iteration of the loop.
						 }
					}
					else
					{
						 // The current line doesn't fit with anything we were expecting to see, so we skip it and try the
						 // next line.
						 line = bufferedReader.ReadLine();
					}
			  }
		 }

		 private RecordType ToRecordType( string entityType )
		 {
			  if ( string.ReferenceEquals( entityType, null ) )
			  {
					// Skip unrecognizable lines.
					return null;
			  }

			  switch ( entityType )
			  {
			  case "Relationship":
					return RecordType.RELATIONSHIP;
			  case "Node":
					return RecordType.NODE;
			  case "Property":
					return RecordType.PROPERTY;
			  case "RelationshipGroup":
					return RecordType.RELATIONSHIP_GROUP;
			  case "IndexRule":
					return RecordType.SCHEMA;
			  case "IndexEntry":
					return RecordType.NODE;
			  default:
					// it's OK, we just haven't implemented support for this yet
					return null;
			  }
		 }

		 private long Id( string line )
		 {
			  int bracket = line.IndexOf( '[' );
			  if ( bracket > -1 )
			  {
					int separator = Min( GetSeparatorIndex( ',', line, bracket ), GetSeparatorIndex( ';', line, bracket ), GetSeparatorIndex( ']', line, bracket ) );
					int equally = line.IndexOf( '=', bracket );
					int startPosition = ( IsNotPlainId( bracket, separator, equally ) ? equally : bracket ) + 1;
					if ( separator > -1 )
					{
						 return long.Parse( line.Substring( startPosition, separator - startPosition ) );
					}
			  }
			  return -1;
		 }

		 private static int Min( params int[] values )
		 {
			  int min = int.MaxValue;
			  foreach ( int value in values )
			  {
					min = Math.Min( min, value );
			  }
			  return min;
		 }

		 private int GetSeparatorIndex( char character, string line, int bracket )
		 {
			  int index = line.IndexOf( character, bracket );
			  return index >= 0 ? index : int.MaxValue;
		 }

		 private bool IsNotPlainId( int bracket, int comma, int equally )
		 {
			  return ( equally > bracket ) && ( equally < comma );
		 }

		 private string EntityType( string line )
		 {
			  int bracket = line.IndexOf( '[' );
			  return bracket == -1 ? null : line.Substring( 0, bracket );
		 }
	}

}
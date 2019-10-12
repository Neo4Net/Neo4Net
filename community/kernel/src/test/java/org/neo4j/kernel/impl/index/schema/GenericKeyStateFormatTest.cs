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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;


	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using FormatCompatibilityVerifier = Org.Neo4j.Test.FormatCompatibilityVerifier;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	public class GenericKeyStateFormatTest : FormatCompatibilityVerifier
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();

		 private const int ENTITY_ID = 19570320;
		 private const int NUMBER_OF_SLOTS = 2;
		 private IList<Value> _values;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _values = new List<Value>();
			  // ZONED_DATE_TIME_ARRAY
			  _values.Add( Values.dateTimeArray( new ZonedDateTime[]{ ZonedDateTime.of( 2018, 10, 9, 8, 7, 6, 5, ZoneId.of( "UTC" ) ), ZonedDateTime.of( 2017, 9, 8, 7, 6, 5, 4, ZoneId.of( "UTC" ) ) } ) );
			  // LOCAL_DATE_TIME_ARRAY
			  _values.Add(Values.localDateTimeArray(new DateTime[]
			  {
				  new DateTime( 2018, 10, 9, 8, 7, 6, 5 ),
				  new DateTime( 2018, 10, 9, 8, 7, 6, 5 )
			  }));
			  // DATE_ARRAY
			  _values.Add( Values.dateArray( new LocalDate[]{ LocalDate.of( 1, 12, 28 ), LocalDate.of( 1, 12, 28 ) } ) );
			  // ZONED_TIME_ARRAY
			  _values.Add( Values.timeArray( new OffsetTime[]{ OffsetTime.of( 19, 8, 7, 6, ZoneOffset.UTC ), OffsetTime.of( 19, 8, 7, 6, ZoneOffset.UTC ) } ) );
			  // LOCAL_TIME_ARRAY
			  _values.Add( Values.localTimeArray( new LocalTime[]{ LocalTime.of( 19, 28 ), LocalTime.of( 19, 28 ) } ) );
			  // DURATION_ARRAY
			  _values.Add( Values.durationArray( new DurationValue[]{ DurationValue.duration( 99, 10, 10, 10 ), DurationValue.duration( 99, 10, 10, 10 ) } ) );
			  // TEXT_ARRAY
			  _values.Add( Values.of( new string[]{ "someString1", "someString2" } ) );
			  // BOOLEAN_ARRAY
			  _values.Add( Values.of( new bool[]{ true, true } ) );
			  // NUMBER_ARRAY (byte, short, int, long, float, double)
			  _values.Add( Values.of( new sbyte[]{ ( sbyte ) 1, ( sbyte ) 12 } ) );
			  _values.Add( Values.of( new short[]{ 314, 1337 } ) );
			  _values.Add( Values.of( new int[]{ 3140, 13370 } ) );
			  _values.Add( Values.of( new long[]{ 31400, 133700 } ) );
			  _values.Add( Values.of( new float[]{ 0.5654f, 13432.14f } ) );
			  _values.Add( Values.of( new double[]{ 432453254.43243, 4354.7888 } ) );
			  _values.Add( Values.of( new char[]{ 'a', 'z' } ) );
			  // ZONED_DATE_TIME
			  _values.Add( DateTimeValue.datetime( 2014, 3, 25, 12, 45, 13, 7474, "UTC" ) );
			  // LOCAL_DATE_TIME
			  _values.Add( LocalDateTimeValue.localDateTime( 2018, 3, 1, 13, 50, 42, 1337 ) );
			  // DATE
			  _values.Add( DateValue.epochDate( 2 ) );
			  // ZONED_TIME
			  _values.Add( TimeValue.time( 43_200_000_000_000L, ZoneOffset.UTC ) );
			  // LOCAL_TIME
			  _values.Add( LocalTimeValue.localTime( 100000 ) );
			  // DURATION
			  _values.Add( DurationValue.duration( 10, 20, 30, 40 ) );
			  // TEXT
			  _values.Add( Values.of( "string1" ) );
			  // BOOLEAN
			  _values.Add( Values.of( true ) );
			  // NUMBER (byte, short, int, long, float, double)
			  _values.Add( Values.of( sbyte.MaxValue ) );
			  _values.Add( Values.of( short.MaxValue ) );
			  _values.Add( Values.of( int.MaxValue ) );
			  _values.Add( Values.of( long.MaxValue ) );
			  _values.Add( Values.of( float.MaxValue ) );
			  _values.Add( Values.of( double.MaxValue ) );
			  _values.Add( Values.of( char.MaxValue ) );
			  // GEOMETRY
			  _values.Add( Values.pointValue( CoordinateReferenceSystem.WGS84, 12.78, 56.7 ) );
			  _values.Add( Values.pointArray( new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.WGS84, 12.7566548, 56.7163465 ), Values.pointValue( CoordinateReferenceSystem.WGS84, 12.13413478, 56.1343457 ) } ) );
			  _values.Add( Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.78, 56.7, 666 ) );
			  _values.Add( Values.pointArray( new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.7566548, 56.7163465, 666 ), Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.13413478, 56.1343457, 555 ) } ) );
			  _values.Add( Values.pointValue( CoordinateReferenceSystem.Cartesian, 0.0000043, -0.0000000012341025786543 ) );
			  _values.Add( Values.pointArray( new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian, 0.0000043, -0.0000000012341025786543 ), Values.pointValue( CoordinateReferenceSystem.Cartesian, 0.2000043, -0.0300000012341025786543 ) } ) );
			  _values.Add( Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 0.0000043, -0.0000000012341025786543, 666 ) );
			  _values.Add( Values.pointArray( new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 0.0000043, -0.0000000012341025786543, 666 ), Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 0.2000043, -0.0300000012341025786543, 555 ) } ) );
		 }

		 protected internal override string ZipName()
		 {
			  return "current-generic-key-state-format.zip";
		 }

		 protected internal override string StoreFileName()
		 {
			  return "generic-key-state-store";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void createStoreFile(java.io.File storeFile) throws java.io.IOException
		 protected internal override void CreateStoreFile( File storeFile )
		 {
			  WithCursor(storeFile, true, c =>
			  {
			  PutFormatVersion( c );
			  PutData( c );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void verifyFormat(java.io.File storeFile) throws FormatViolationException, java.io.IOException
		 protected internal override void VerifyFormat( File storeFile )
		 {
			  AtomicReference<FormatViolationException> exception = new AtomicReference<FormatViolationException>();
			  WithCursor(storeFile, false, c =>
			  {
				int major = c.Int;
				int minor = c.Int;
				GenericLayout layout = Layout;
				if ( major != layout.MajorVersion() || minor != layout.MinorVersion() )
				{
					 exception.set( new FormatViolationException( this, string.Format( "Read format version {0:D}.{1:D}, but layout has version {2:D}.{3:D}", major, minor, layout.MajorVersion(), layout.MinorVersion() ) ) );
				}
			  });
			  if ( exception.get() != null )
			  {
					throw exception.get();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void verifyContent(java.io.File storeFile) throws java.io.IOException
		 protected internal override void VerifyContent( File storeFile )
		 {
			  WithCursor(storeFile, false, c =>
			  {
				ReadFormatVersion( c );
				VerifyData( c );
			  });
		 }

		 private void PutFormatVersion( PageCursor cursor )
		 {
			  GenericLayout layout = Layout;
			  int major = layout.MajorVersion();
			  cursor.PutInt( major );
			  int minor = layout.MinorVersion();
			  cursor.PutInt( minor );
		 }

		 private void ReadFormatVersion( PageCursor c )
		 {
			  c.Int; // Major version
			  c.Int; // Minor version
		 }

		 private void PutData( PageCursor c )
		 {
			  GenericLayout layout = Layout;
			  GenericKey key = layout.NewKey();
			  foreach ( Value value in _values )
			  {
					InitializeFromValue( key, value );
					c.PutInt( key.Size() );
					layout.WriteKey( c, key );
			  }
		 }

		 private void InitializeFromValue( GenericKey key, Value value )
		 {
			  key.Initialize( ENTITY_ID );
			  for ( int i = 0; i < NUMBER_OF_SLOTS; i++ )
			  {
					key.InitFromValue( i, value, NativeIndexKey.Inclusion.Neutral );
			  }
		 }

		 private void VerifyData( PageCursor c )
		 {
			  GenericLayout layout = Layout;
			  GenericKey readCompositeKey = layout.NewKey();
			  GenericKey comparison = layout.NewKey();
			  foreach ( Value value in _values )
			  {
					int keySize = c.Int;
					layout.ReadKey( c, readCompositeKey, keySize );
					foreach ( Value readValue in readCompositeKey.AsValues() )
					{
						 InitializeFromValue( comparison, value );
						 assertEquals( 0, layout.Compare( readCompositeKey, comparison ), DetailedFailureMessage( readCompositeKey, comparison ) );
						 if ( readValue != Values.NO_VALUE )
						 {
							  assertEquals( value, readValue, "expected read value to be " + value + ", but was " + readValue );
						 }
					}
			  }
		 }

		 private string DetailedFailureMessage( GenericKey actualKey, GenericKey expectedKey )
		 {
			  return "expected " + expectedKey.ToDetailedString() + ", but was " + actualKey.ToDetailedString();
		 }

		 private GenericLayout Layout
		 {
			 get
			 {
				  return new GenericLayout( NUMBER_OF_SLOTS, new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>() ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void withCursor(java.io.File storeFile, boolean create, System.Action<org.neo4j.io.pagecache.PageCursor> cursorConsumer) throws java.io.IOException
		 private void WithCursor( File storeFile, bool create, System.Action<PageCursor> cursorConsumer )
		 {
			  OpenOption[] openOptions = create ? new OpenOption[]{ StandardOpenOption.WRITE, StandardOpenOption.CREATE } : new OpenOption[]{ StandardOpenOption.WRITE };
			  using ( PageCache pageCache = PageCacheRule.getPageCache( GlobalFs.get() ), PagedFile pagedFile = pageCache.Map(storeFile, pageCache.PageSize(), openOptions), PageCursor cursor = pagedFile.Io(0, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock) )
			  {
					cursor.Next();
					cursorConsumer( cursor );
			  }
		 }
	}

}
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
namespace Org.Neo4j.Kernel.impl.store
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ShortStringPropertyEncodeTest
	{
		 private const int KEY_ID = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

		 private NeoStores _neoStores;
		 private PropertyStore _propertyStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupStore()
		 public virtual void SetupStore()
		 {
			  _neoStores = ( new StoreFactory( Storage.directory().databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(Storage.fileSystem()), Storage.pageCache(), Storage.fileSystem(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY ) ).openNeoStores(true, StoreType.Property, StoreType.PropertyArray, StoreType.PropertyString);
			  _propertyStore = _neoStores.PropertyStore;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeStore()
		 public virtual void CloseStore()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeEmptyString()
		 public virtual void CanEncodeEmptyString()
		 {
			  AssertCanEncode( "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeReallyLongString()
		 public virtual void CanEncodeReallyLongString()
		 {
			  AssertCanEncode( "                    " ); // 20 spaces
			  AssertCanEncode( "                " ); // 16 spaces
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeFifteenSpaces()
		 public virtual void CanEncodeFifteenSpaces()
		 {
			  AssertCanEncode( "               " );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeNumericalString()
		 public virtual void CanEncodeNumericalString()
		 {
			  AssertCanEncode( "0123456789+,'.-" );
			  AssertCanEncode( " ,'.-0123456789" );
			  AssertCanEncode( "+ '.0123456789-" );
			  AssertCanEncode( "+, 0123456789.-" );
			  AssertCanEncode( "+,0123456789' -" );
			  AssertCanEncode( "+0123456789,'. " );
			  // IP(v4) numbers
			  AssertCanEncode( "192.168.0.1" );
			  AssertCanEncode( "127.0.0.1" );
			  AssertCanEncode( "255.255.255.255" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeTooLongStringsWithCharsInDifferentTables()
		 public virtual void CanEncodeTooLongStringsWithCharsInDifferentTables()
		 {
			  AssertCanEncode( "____________+" );
			  AssertCanEncode( "_____+_____" );
			  AssertCanEncode( "____+____" );
			  AssertCanEncode( "HELLO world" );
			  AssertCanEncode( "Hello_World" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeUpToNineEuropeanChars()
		 public virtual void CanEncodeUpToNineEuropeanChars()
		 {
			  // Shorter than 10 chars
			  AssertCanEncode( "fågel" ); // "bird" in Swedish
			  AssertCanEncode( "påfågel" ); // "peacock" in Swedish
			  AssertCanEncode( "påfågelö" ); // "peacock island" in Swedish
			  AssertCanEncode( "påfågelön" ); // "the peacock island" in Swedish
			  // 10 chars
			  AssertCanEncode( "påfågelöar" ); // "peacock islands" in Swedish
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeEuropeanCharsWithPunctuation()
		 public virtual void CanEncodeEuropeanCharsWithPunctuation()
		 {
			  AssertCanEncode( "qHm7 pp3" );
			  AssertCanEncode( "UKKY3t.gk" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeAlphanumerical()
		 public virtual void CanEncodeAlphanumerical()
		 {
			  AssertCanEncode( "1234567890" ); // Just a sanity check
			  AssertCanEncodeInBothCasings( "HelloWor1d" ); // There is a number there
			  AssertCanEncode( "          " ); // Alphanum is the first that can encode 10 spaces
			  AssertCanEncode( "_ _ _ _ _ " ); // The only available punctuation
			  AssertCanEncode( "H3Lo_ or1D" ); // Mixed case + punctuation
			  AssertCanEncode( "q1w2e3r4t+" ); // + is not in the charset
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeHighUnicode()
		 public virtual void CanEncodeHighUnicode()
		 {
			  AssertCanEncode( "\u02FF" );
			  AssertCanEncode( "hello\u02FF" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeLatin1SpecialChars()
		 public virtual void CanEncodeLatin1SpecialChars()
		 {
			  AssertCanEncode( "#$#$#$#" );
			  AssertCanEncode( "$hello#" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeTooLongLatin1String()
		 public virtual void CanEncodeTooLongLatin1String()
		 {
			  AssertCanEncode( "#$#$#$#$" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeLowercaseAndUppercaseStringsUpTo12Chars()
		 public virtual void CanEncodeLowercaseAndUppercaseStringsUpTo12Chars()
		 {
			  AssertCanEncodeInBothCasings( "hello world" );
			  AssertCanEncode( "hello_world" );
			  AssertCanEncode( "_hello_world" );
			  AssertCanEncode( "hello::world" );
			  AssertCanEncode( "hello//world" );
			  AssertCanEncode( "hello world" );
			  AssertCanEncode( "http://ok" );
			  AssertCanEncode( "::::::::" );
			  AssertCanEncode( " _.-:/ _.-:/" );
		 }

		 private void AssertCanEncodeInBothCasings( string @string )
		 {
			  AssertCanEncode( @string.ToLower() );
			  AssertCanEncode( @string.ToUpper() );
		 }

		 private void AssertCanEncode( string @string )
		 {
			  Encode( @string );
		 }

		 private void Encode( string @string )
		 {
			  PropertyBlock block = new PropertyBlock();
			  TextValue expectedValue = Values.stringValue( @string );
			  _propertyStore.encodeValue( block, KEY_ID, expectedValue );
			  assertEquals( 0, block.ValueRecords.Count );
			  Value readValue = block.Type.value( block, _propertyStore );
			  assertEquals( expectedValue, readValue );
		 }
	}

}
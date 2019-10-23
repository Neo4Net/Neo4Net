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
namespace Neo4Net.tools.dump
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using NamedToken = Neo4Net.Kernel.Api.Internal.NamedToken;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using LabelTokenStore = Neo4Net.Kernel.impl.store.LabelTokenStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyKeyTokenStore = Neo4Net.Kernel.impl.store.PropertyKeyTokenStore;
	using RelationshipTypeTokenStore = Neo4Net.Kernel.impl.store.RelationshipTypeTokenStore;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using BigEndianByteArrayBuffer = Neo4Net.Kernel.impl.store.kvstore.BigEndianByteArrayBuffer;
	using Neo4Net.Kernel.impl.store.kvstore;
	using Headers = Neo4Net.Kernel.impl.store.kvstore.Headers;
	using ReadableBuffer = Neo4Net.Kernel.impl.store.kvstore.ReadableBuffer;
	using WritableBuffer = Neo4Net.Kernel.impl.store.kvstore.WritableBuffer;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DumpCountsStoreTest
	{
		 private const int START_LABEL_ID = 1;
		 private const int END_LABEL_ID = 2;
		 private const int INDEX_LABEL_ID = 3;
		 private const int NODE_LABEL_ID = 10;
		 private const string TEST_LABEL = "testLabel";
		 private const string START_LABEL = "startLabel";
		 private const string END_LABEL = "endLabel";
		 private const string INDEX_LABEL = "indexLabel";

		 private const int TYPE_ID = 1;
		 private const string TYPE_LABEL = "testType";

		 private const int INDEX_PROPERTY_KEY_ID = 1;
		 private const string INDEX_PROPERTY = "indexProperty";

		 private const long INDEX_ID = 0;
		 private static readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( INDEX_LABEL_ID, INDEX_PROPERTY_KEY_ID );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dumpMetadata()
		 public virtual void DumpMetadata()
		 {
			  DumpCountsStore countsStore = CountStore;

			  Headers headers = Headers.headersBuilder().put(CreateNamedHeader("header1"), "value1").put(CreateNamedHeader("header2"), "value2").headers();
			  File file = mock( typeof( File ) );
			  when( file.ToString() ).thenReturn("Test File");

			  countsStore.VisitMetadata( file, headers, 100 );
			  assertThat( SuppressOutput.OutputVoice.ToString(), allOf(containsString("Counts Store:\tTest File"), containsString("header2:\tvalue2"), containsString("header1:\tvalue1"), containsString("entries:\t100")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dumpNodeCount()
		 public virtual void DumpNodeCount()
		 {
			  DumpCountsStore countsStore = CountStore;
			  countsStore.VisitNodeCount( NODE_LABEL_ID, 70 );
			  assertThat( SuppressOutput.OutputVoice.ToString(), containsString("Node[(:testLabel [labelId=10])]:\t70") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dumpRelationshipCount()
		 public virtual void DumpRelationshipCount()
		 {
			  DumpCountsStore countsStore = CountStore;
			  countsStore.VisitRelationshipCount( START_LABEL_ID, TYPE_ID, END_LABEL_ID, 5 );
			  assertThat( SuppressOutput.OutputVoice.ToString(), containsString("\tRelationship[(:startLabel [labelId=1])-[:testType [typeId=1]]->" + "(:endLabel [labelId=2])]:\t5") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dumpUnknownKey()
		 public virtual void DumpUnknownKey()
		 {
			  DumpCountsStore countsStore = CountStore;
			  countsStore.VisitUnknownKey( new BigEndianByteArrayBuffer( "unknownKey".GetBytes() ), new BigEndianByteArrayBuffer("unknownValue".GetBytes()) );
			  assertThat( SuppressOutput.OutputVoice.ToString(), containsString("['u', 'n', 'k', 'n', 'o', 'w', 'n', 'K', 'e', 'y']:\t['u', 'n', 'k', 'n', 'o', " + "'w', 'n', 'V', 'a', 'l', 'u', 'e']") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dumpIndexStatistic()
		 public virtual void DumpIndexStatistic()
		 {
			  DumpCountsStore countsStore = CountStore;
			  countsStore.VisitIndexStatistics( INDEX_ID, 3, 4 );
			  assertThat( SuppressOutput.OutputVoice.ToString(), containsString("IndexStatistics[(:indexLabel [labelId=3] {indexProperty [keyId=1]})" + "]:\tupdates=3, size=4") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dumpIndexSample()
		 public virtual void DumpIndexSample()
		 {
			  DumpCountsStore countsStore = CountStore;
			  countsStore.VisitIndexSample( INDEX_ID, 1, 2 );
			  assertThat( SuppressOutput.OutputVoice.ToString(), containsString("IndexSample[(:indexLabel [labelId=3] {indexProperty [keyId=1]})]:\tunique=1, size=2") );
		 }

		 private DumpCountsStore CountStore
		 {
			 get
			 {
				  return new DumpCountsStore( System.out, CreateNeoStores(), CreateSchemaStorage() );
			 }
		 }

		 private SchemaStorage CreateSchemaStorage()
		 {
			  SchemaStorage schemaStorage = mock( typeof( SchemaStorage ) );
			  StoreIndexDescriptor rule = _descriptor.withId( INDEX_ID );
			  List<StoreIndexDescriptor> rules = new List<StoreIndexDescriptor>();
			  rules.Add( rule );

			  when( schemaStorage.IndexesGetAll() ).thenReturn(rules.GetEnumerator());
			  return schemaStorage;
		 }

		 private NeoStores CreateNeoStores()
		 {
			  NeoStores neoStores = mock( typeof( NeoStores ) );
			  LabelTokenStore labelTokenStore = mock( typeof( LabelTokenStore ) );
			  RelationshipTypeTokenStore typeTokenStore = mock( typeof( RelationshipTypeTokenStore ) );
			  PropertyKeyTokenStore propertyKeyTokenStore = mock( typeof( PropertyKeyTokenStore ) );

			  when( labelTokenStore.Tokens ).thenReturn( LabelTokens );
			  when( typeTokenStore.Tokens ).thenReturn( TypeTokes );
			  when( propertyKeyTokenStore.Tokens ).thenReturn( PropertyTokens );

			  when( neoStores.LabelTokenStore ).thenReturn( labelTokenStore );
			  when( neoStores.RelationshipTypeTokenStore ).thenReturn( typeTokenStore );
			  when( neoStores.PropertyKeyTokenStore ).thenReturn( propertyKeyTokenStore );

			  return neoStores;
		 }

		 private IList<NamedToken> PropertyTokens
		 {
			 get
			 {
				  return Collections.singletonList( new NamedToken( INDEX_PROPERTY, INDEX_PROPERTY_KEY_ID ) );
			 }
		 }

		 private IList<NamedToken> TypeTokes
		 {
			 get
			 {
				  return Collections.singletonList( new NamedToken( TYPE_LABEL, TYPE_ID ) );
			 }
		 }

		 private IList<NamedToken> LabelTokens
		 {
			 get
			 {
				  return Arrays.asList( new NamedToken( START_LABEL, START_LABEL_ID ), new NamedToken( END_LABEL, END_LABEL_ID ), new NamedToken( INDEX_LABEL, INDEX_LABEL_ID ), new NamedToken( TEST_LABEL, NODE_LABEL_ID ) );
			 }
		 }

		 private HeaderField<string> CreateNamedHeader( string name )
		 {
			  return new HeaderFieldAnonymousInnerClass( this, name );
		 }

		 private class HeaderFieldAnonymousInnerClass : HeaderField<string>
		 {
			 private readonly DumpCountsStoreTest _outerInstance;

			 private string _name;

			 public HeaderFieldAnonymousInnerClass( DumpCountsStoreTest outerInstance, string name )
			 {
				 this.outerInstance = outerInstance;
				 this._name = name;
			 }

			 public string read( ReadableBuffer header )
			 {
				  return _name;
			 }

			 public void write( string s, WritableBuffer header )
			 {

			 }

			 public override string ToString()
			 {
				  return _name;
			 }
		 }
	}

}
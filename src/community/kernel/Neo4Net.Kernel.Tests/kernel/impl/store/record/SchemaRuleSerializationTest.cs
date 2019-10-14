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
namespace Neo4Net.Kernel.Impl.Store.Records
{
	using Test = org.junit.Test;


	using MalformedSchemaRuleException = Neo4Net.Internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class SchemaRuleSerializationTest : SchemaRuleTestBase
	{
		 internal StoreIndexDescriptor IndexRegular = ForLabel( LABEL_ID, PROPERTY_ID_1 ).withId( RULE_ID );

		 internal StoreIndexDescriptor IndexUnique = UniqueForLabel( LABEL_ID, PROPERTY_ID_1 ).withIds( RULE_ID_2, RULE_ID );

		 internal StoreIndexDescriptor IndexCompositeRegular = ForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ).withId( RULE_ID );

		 internal StoreIndexDescriptor IndexMultiTokenRegular = forSchema( multiToken( new int[]{ LABEL_ID, LABEL_ID_2 }, EntityType.NODE, new int[]{ PROPERTY_ID_1, PROPERTY_ID_2 } ) ).withId( RULE_ID );

		 internal StoreIndexDescriptor IndexCompositeUnique = UniqueForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ).withIds( RULE_ID_2, RULE_ID );

		 internal StoreIndexDescriptor IndexBigComposite = ForLabel( LABEL_ID, IntStream.range( 1, 200 ).toArray() ).withId(RULE_ID);

		 internal StoreIndexDescriptor IndexBigMultiToken = forSchema( multiToken( IntStream.range( 1, 200 ).toArray(), EntityType.RELATIONSHIP, IntStream.range(1, 200).toArray() ) ).withId(RULE_ID);

		 internal ConstraintRule ConstraintExistsLabel = ConstraintRule.ConstraintRuleConflict( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1 ) );

		 internal ConstraintRule ConstraintUniqueLabel = ConstraintRule.constraintRule( RULE_ID_2, ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID );

		 internal ConstraintRule ConstraintNodeKeyLabel = ConstraintRule.constraintRule( RULE_ID_2, ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID );

		 internal ConstraintRule ConstraintExistsRelType = ConstraintRule.ConstraintRuleConflict( RULE_ID_2, ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1 ) );

		 internal ConstraintRule ConstraintCompositeLabel = ConstraintRule.ConstraintRuleConflict( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );

		 internal ConstraintRule ConstraintCompositeRelType = ConstraintRule.ConstraintRuleConflict( RULE_ID_2, ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );

		 // INDEX RULES

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rulesCreatedWithoutNameMustHaveComputedName()
		 public virtual void RulesCreatedWithoutNameMustHaveComputedName()
		 {
			  assertThat( IndexRegular.Name, @is( "index_1" ) );
			  assertThat( IndexUnique.Name, @is( "index_2" ) );
			  assertThat( IndexCompositeRegular.Name, @is( "index_1" ) );
			  assertThat( IndexCompositeUnique.Name, @is( "index_2" ) );
			  assertThat( IndexBigComposite.Name, @is( "index_1" ) );
			  assertThat( ConstraintExistsLabel.Name, @is( "constraint_1" ) );
			  assertThat( ConstraintUniqueLabel.Name, @is( "constraint_2" ) );
			  assertThat( ConstraintNodeKeyLabel.Name, @is( "constraint_2" ) );
			  assertThat( ConstraintExistsRelType.Name, @is( "constraint_2" ) );
			  assertThat( ConstraintCompositeLabel.Name, @is( "constraint_1" ) );
			  assertThat( ConstraintCompositeRelType.Name, @is( "constraint_2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rulesCreatedWithoutNameMustRetainComputedNameAfterDeserialisation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RulesCreatedWithoutNameMustRetainComputedNameAfterDeserialisation()
		 {
			  assertThat( SerialiseAndDeserialise( IndexRegular ).Name, @is( "index_1" ) );
			  assertThat( SerialiseAndDeserialise( IndexUnique ).Name, @is( "index_2" ) );
			  assertThat( SerialiseAndDeserialise( IndexCompositeRegular ).Name, @is( "index_1" ) );
			  assertThat( SerialiseAndDeserialise( IndexCompositeUnique ).Name, @is( "index_2" ) );
			  assertThat( SerialiseAndDeserialise( IndexBigComposite ).Name, @is( "index_1" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintExistsLabel ).Name, @is( "constraint_1" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintUniqueLabel ).Name, @is( "constraint_2" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintNodeKeyLabel ).Name, @is( "constraint_2" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintExistsRelType ).Name, @is( "constraint_2" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintCompositeLabel ).Name, @is( "constraint_1" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintCompositeRelType ).Name, @is( "constraint_2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rulesCreatedWithNameMustRetainGivenNameAfterDeserialisation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RulesCreatedWithNameMustRetainGivenNameAfterDeserialisation()
		 {
			  string name = "custom_rule";

			  assertThat( SerialiseAndDeserialise( NamedForLabel( name, LABEL_ID, PROPERTY_ID_1 ).withId( RULE_ID ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( NamedUniqueForLabel( name, LABEL_ID, PROPERTY_ID_1 ).withIds( RULE_ID_2, RULE_ID ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( NamedForLabel( name, LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ).withId( RULE_ID ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( NamedUniqueForLabel( name, LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ).withIds( RULE_ID_2, RULE_ID ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( NamedForLabel( name, LABEL_ID, IntStream.range( 1, 200 ).toArray() ).withId(RULE_ID) ).Name, @is(name) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1 ), name ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.ConstraintRuleConflict( RULE_ID_2, ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID, name ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.ConstraintRuleConflict( RULE_ID_2, ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID, name ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID_2, ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1 ), name ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ), name ) ).Name, @is( name ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID_2, ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1, PROPERTY_ID_2 ), name ) ).Name, @is( name ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rulesCreatedWithNullNameMustRetainComputedNameAfterDeserialisation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RulesCreatedWithNullNameMustRetainComputedNameAfterDeserialisation()
		 {
			  assertThat( SerialiseAndDeserialise( ForLabel( LABEL_ID, PROPERTY_ID_1 ).withId( RULE_ID ) ).Name, @is( "index_1" ) );
			  assertThat( SerialiseAndDeserialise( UniqueForLabel( LABEL_ID, PROPERTY_ID_1 ).withIds( RULE_ID_2, RULE_ID ) ).Name, @is( "index_2" ) );
			  assertThat( SerialiseAndDeserialise( ForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ).withId( RULE_ID ) ).Name, @is( "index_1" ) );
			  assertThat( SerialiseAndDeserialise( UniqueForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ).withIds( RULE_ID_2, RULE_ID ) ).Name, @is( "index_2" ) );
			  assertThat( SerialiseAndDeserialise( ForLabel( LABEL_ID, IntStream.range( 1, 200 ).toArray() ).withId(RULE_ID) ).Name, @is("index_1") );

			  string name = null;
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1 ), name ) ).Name, @is( "constraint_1" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.ConstraintRuleConflict( RULE_ID_2, ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID, name ) ).Name, @is( "constraint_2" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.ConstraintRuleConflict( RULE_ID_2, ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID, name ) ).Name, @is( "constraint_2" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID_2, ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1 ), name ) ).Name, @is( "constraint_2" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ), name ) ).Name, @is( "constraint_1" ) );
			  assertThat( SerialiseAndDeserialise( ConstraintRule.constraintRule( RULE_ID_2, ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1, PROPERTY_ID_2 ), name ) ).Name, @is( "constraint_2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void indexRuleNameMustNotContainNullCharacter()
		 public virtual void IndexRuleNameMustNotContainNullCharacter()
		 {
			  string name = "a\0b";
			  NamedForLabel( name, LABEL_ID, PROPERTY_ID_1 ).withId( RULE_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void indexRuleNameMustNotBeTheEmptyString()
		 public virtual void IndexRuleNameMustNotBeTheEmptyString()
		 {
			  //noinspection RedundantStringConstructorCall
			  string name = "";
			  NamedForLabel( name, LABEL_ID, PROPERTY_ID_1 ).withId( RULE_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void constraintIndexRuleNameMustNotContainNullCharacter()
		 public virtual void ConstraintIndexRuleNameMustNotContainNullCharacter()
		 {
			  string name = "a\0b";
			  NamedUniqueForLabel( name, LABEL_ID, PROPERTY_ID_1 ).withIds( RULE_ID, RULE_ID_2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void constraintIndexRuleNameMustNotBeTheEmptyString()
		 public virtual void ConstraintIndexRuleNameMustNotBeTheEmptyString()
		 {
			  //noinspection RedundantStringConstructorCall
			  string name = "";
			  NamedUniqueForLabel( name, LABEL_ID, PROPERTY_ID_1 ).withIds( RULE_ID, RULE_ID_2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void constraintRuleNameMustNotContainNullCharacter()
		 public virtual void ConstraintRuleNameMustNotContainNullCharacter()
		 {
			  string name = "a\0b";
			  ConstraintRule.constraintRule( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1 ), name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void constraintRuleNameMustNotBeTheEmptyString()
		 public virtual void ConstraintRuleNameMustNotBeTheEmptyString()
		 {
			  //noinspection RedundantStringConstructorCall
			  string name = "";
			  ConstraintRule.constraintRule( RULE_ID, ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1 ), name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void uniquenessConstraintRuleNameMustNotContainNullCharacter()
		 public virtual void UniquenessConstraintRuleNameMustNotContainNullCharacter()
		 {
			  string name = "a\0b";
			  ConstraintRule.ConstraintRuleConflict( RULE_ID, ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID_2, name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void uniquenessConstraintRuleNameMustNotBeTheEmptyString()
		 public virtual void UniquenessConstraintRuleNameMustNotBeTheEmptyString()
		 {
			  //noinspection RedundantStringConstructorCall
			  string name = "";
			  ConstraintRule.ConstraintRuleConflict( RULE_ID, ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID_2, name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void nodeKeyConstraintRuleNameMustNotContainNullCharacter()
		 public virtual void NodeKeyConstraintRuleNameMustNotContainNullCharacter()
		 {
			  string name = "a\0b";
			  ConstraintRule.ConstraintRuleConflict( RULE_ID, ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID_2, name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void nodeKeyConstraintRuleNameMustNotBeTheEmptyString()
		 public virtual void NodeKeyConstraintRuleNameMustNotBeTheEmptyString()
		 {
			  //noinspection RedundantStringConstructorCall
			  string name = "";
			  ConstraintRule.ConstraintRuleConflict( RULE_ID, ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 ), RULE_ID_2, name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeIndexRules() throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeIndexRules()
		 {
			  AssertSerializeAndDeserializeIndexRule( IndexRegular );
			  AssertSerializeAndDeserializeIndexRule( IndexUnique );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeCompositeIndexRules() throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeCompositeIndexRules()
		 {
			  AssertSerializeAndDeserializeIndexRule( IndexCompositeRegular );
			  AssertSerializeAndDeserializeIndexRule( IndexCompositeUnique );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserialize_Big_CompositeIndexRules() throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeBigCompositeIndexRules()
		 {
			  AssertSerializeAndDeserializeIndexRule( IndexBigComposite );
		 }

		 // CONSTRAINT RULES

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeConstraintRules() throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeConstraintRules()
		 {
			  AssertSerializeAndDeserializeConstraintRule( ConstraintExistsLabel );
			  AssertSerializeAndDeserializeConstraintRule( ConstraintUniqueLabel );
			  AssertSerializeAndDeserializeConstraintRule( ConstraintNodeKeyLabel );
			  AssertSerializeAndDeserializeConstraintRule( ConstraintExistsRelType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeCompositeConstraintRules() throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeCompositeConstraintRules()
		 {
			  AssertSerializeAndDeserializeConstraintRule( ConstraintCompositeLabel );
			  AssertSerializeAndDeserializeConstraintRule( ConstraintCompositeRelType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeMultiTokenRules() throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeMultiTokenRules()
		 {
			  AssertSerializeAndDeserializeIndexRule( IndexMultiTokenRegular );
			  AssertSerializeAndDeserializeIndexRule( IndexBigMultiToken );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectLengthForIndexRules()
		 public virtual void ShouldReturnCorrectLengthForIndexRules()
		 {
			  AssertCorrectLength( IndexRegular );
			  AssertCorrectLength( IndexUnique );
			  AssertCorrectLength( IndexCompositeRegular );
			  AssertCorrectLength( IndexCompositeUnique );
			  AssertCorrectLength( IndexBigComposite );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectLengthForConstraintRules()
		 public virtual void ShouldReturnCorrectLengthForConstraintRules()
		 {
			  AssertCorrectLength( ConstraintExistsLabel );
		 }

		 // BACKWARDS COMPATIBILITY

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseIndexRule()
		 {
			  AssertParseIndexRule( "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABA==", "index_24" );
			  AssertParseIndexRule( "AAACAAEAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMAABAAAAAAAAAAQ=", "index_24" ); // LEGACY
			  AssertParseIndexRule( "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABAAAAAtjdXN0b21fbmFtZQ==", "custom_name" ); // named rule
			  AssertParseIndexRule( AddNullByte( "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABA==" ), "index_24" ); // empty name
			  AssertParseIndexRule( AddNullByte( 2, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABA==" ), "index_24" ); // empty name
			  AssertParseIndexRule( AddNullByte( 3, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABA==" ), "index_24" ); // empty name
			  AssertParseIndexRule( AddNullByte( 4, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABA==" ), "index_24" ); // empty name
			  AssertParseIndexRule( AddNullByte( 5, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMB9bAAACAAABAAAABA==" ), "index_24" ); // empty name
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseUniqueIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseUniqueIndexRule()
		 {
			  AssertParseUniqueIndexRule( "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPc", "index_33" );
			  AssertParseUniqueIndexRule( "AAAAPQIAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMAABAAAAAAAAA9wAAAAAAAAACw==", "index_33" ); // LEGACY
			  AssertParseUniqueIndexRule( "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPcAAAAC2N1c3RvbV9uYW1l", "custom_name" ); // named rule
			  AssertParseUniqueIndexRule( AddNullByte( "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPc" ), "index_33" ); // empty name
			  AssertParseUniqueIndexRule( AddNullByte( 2, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPc" ), "index_33" ); // empty name
			  AssertParseUniqueIndexRule( AddNullByte( 3, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPc" ), "index_33" ); // empty name
			  AssertParseUniqueIndexRule( AddNullByte( 4, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPc" ), "index_33" ); // empty name
			  AssertParseUniqueIndexRule( AddNullByte( 5, "/////wsAAAAOaW5kZXgtcHJvdmlkZXIAAAAEMjUuMCAAAAAAAAAAC1sAAAA9AAEAAAPc" ), "index_33" ); // empty name
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseUniqueConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseUniqueConstraintRule()
		 {
			  AssertParseUniqueConstraintRule( "/////ww+AAAAAAAAAAJbAAAANwABAAAAAw==", "constraint_1" );
			  AssertParseUniqueConstraintRule( "AAAANwMBAAAAAAAAAAMAAAAAAAAAAg==", "constraint_1" ); // LEGACY
			  AssertParseUniqueConstraintRule( "/////ww+AAAAAAAAAAJbAAAANwABAAAAAwAAAAtjdXN0b21fbmFtZQ==", "custom_name" ); // named rule
			  AssertParseUniqueConstraintRule( AddNullByte( "/////ww+AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseUniqueConstraintRule( AddNullByte( 2, "/////ww+AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseUniqueConstraintRule( AddNullByte( 3, "/////ww+AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseUniqueConstraintRule( AddNullByte( 4, "/////ww+AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseUniqueConstraintRule( AddNullByte( 5, "/////ww+AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseNodeKeyConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseNodeKeyConstraintRule()
		 {
			  AssertParseNodeKeyConstraintRule( "/////ww/AAAAAAAAAAJbAAAANwABAAAAAw==", "constraint_1" );
			  AssertParseNodeKeyConstraintRule( "/////ww/AAAAAAAAAAJbAAAANwABAAAAAwAAAAtjdXN0b21fbmFtZQ==", "custom_name" ); // named rule
			  AssertParseNodeKeyConstraintRule( AddNullByte( "/////ww/AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseNodeKeyConstraintRule( AddNullByte( 2, "/////ww/AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseNodeKeyConstraintRule( AddNullByte( 3, "/////ww/AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseNodeKeyConstraintRule( AddNullByte( 4, "/////ww/AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
			  AssertParseNodeKeyConstraintRule( AddNullByte( 5, "/////ww/AAAAAAAAAAJbAAAANwABAAAAAw==" ), "constraint_1" ); // empty name
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseNodePropertyExistsRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseNodePropertyExistsRule()
		 {
			  AssertParseNodePropertyExistsRule( "/////ww9WwAAAC0AAQAAADM=", "constraint_87" );
			  AssertParseNodePropertyExistsRule( "AAAALQQAAAAz", "constraint_87" ); // LEGACY
			  AssertParseNodePropertyExistsRule( "/////ww9WwAAAC0AAQAAADMAAAALY3VzdG9tX25hbWU=", "custom_name" ); // named rule
			  AssertParseNodePropertyExistsRule( AddNullByte( "/////ww9WwAAAC0AAQAAADM=" ), "constraint_87" ); // empty name
			  AssertParseNodePropertyExistsRule( AddNullByte( 2, "/////ww9WwAAAC0AAQAAADM=" ), "constraint_87" ); // empty name
			  AssertParseNodePropertyExistsRule( AddNullByte( 3, "/////ww9WwAAAC0AAQAAADM=" ), "constraint_87" ); // empty name
			  AssertParseNodePropertyExistsRule( AddNullByte( 4, "/////ww9WwAAAC0AAQAAADM=" ), "constraint_87" ); // empty name
			  AssertParseNodePropertyExistsRule( AddNullByte( 5, "/////ww9WwAAAC0AAQAAADM=" ), "constraint_87" ); // empty name
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseRelationshipPropertyExistsRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseRelationshipPropertyExistsRule()
		 {
			  AssertParseRelationshipPropertyExistsRule( "/////ww9XAAAIUAAAQAAF+c=", "constraint_51" );
			  AssertParseRelationshipPropertyExistsRule( "AAAhQAUAABfn", "constraint_51" ); // LEGACY6
			  AssertParseRelationshipPropertyExistsRule( "/////ww9XAAAIUAAAQAAF+cAAAALY3VzdG9tX25hbWU=", "custom_name" ); // named rule
			  AssertParseRelationshipPropertyExistsRule( AddNullByte( "/////ww9XAAAIUAAAQAAF+c=" ), "constraint_51" ); // empty name
			  AssertParseRelationshipPropertyExistsRule( AddNullByte( 2, "/////ww9XAAAIUAAAQAAF+c=" ), "constraint_51" ); // empty name
			  AssertParseRelationshipPropertyExistsRule( AddNullByte( 3, "/////ww9XAAAIUAAAQAAF+c=" ), "constraint_51" ); // empty name
			  AssertParseRelationshipPropertyExistsRule( AddNullByte( 4, "/////ww9XAAAIUAAAQAAF+c=" ), "constraint_51" ); // empty name
			  AssertParseRelationshipPropertyExistsRule( AddNullByte( 5, "/////ww9XAAAIUAAAQAAF+c=" ), "constraint_51" ); // empty name
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertParseIndexRule(String serialized, String name) throws Exception
		 private void AssertParseIndexRule( string serialized, string name )
		 {
			  // GIVEN
			  long ruleId = 24;
			  IndexDescriptor index = ForLabel( 512, 4 );
			  IndexProviderDescriptor indexProvider = new IndexProviderDescriptor( "index-provider", "25.0" );
			  sbyte[] bytes = DecodeBase64( serialized );

			  // WHEN
			  StoreIndexDescriptor deserialized = AssertIndexRule( SchemaRuleSerialization.Deserialize( ruleId, ByteBuffer.wrap( bytes ) ) );

			  // THEN
			  assertThat( deserialized.Id, equalTo( ruleId ) );
			  assertThat( deserialized, equalTo( index ) );
			  assertThat( deserialized.Schema(), equalTo(index.Schema()) );
			  assertThat( deserialized.ProviderDescriptor(), equalTo(indexProvider) );
			  assertThat( deserialized.Name, @is( name ) );
			  assertException( deserialized.getOwningConstraint, typeof( System.InvalidOperationException ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertParseUniqueIndexRule(String serialized, String name) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private void AssertParseUniqueIndexRule( string serialized, string name )
		 {
			  // GIVEN
			  long ruleId = 33;
			  long constraintId = 11;
			  IndexDescriptor index = TestIndexDescriptorFactory.uniqueForLabel( 61, 988 );
			  IndexProviderDescriptor indexProvider = new IndexProviderDescriptor( "index-provider", "25.0" );
			  sbyte[] bytes = DecodeBase64( serialized );

			  // WHEN
			  StoreIndexDescriptor deserialized = AssertIndexRule( SchemaRuleSerialization.Deserialize( ruleId, ByteBuffer.wrap( bytes ) ) );

			  // THEN
			  assertThat( deserialized.Id, equalTo( ruleId ) );
			  assertThat( deserialized, equalTo( index ) );
			  assertThat( deserialized.Schema(), equalTo(index.Schema()) );
			  assertThat( deserialized.ProviderDescriptor(), equalTo(indexProvider) );
			  assertThat( deserialized.OwningConstraint, equalTo( constraintId ) );
			  assertThat( deserialized.Name, @is( name ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertParseUniqueConstraintRule(String serialized, String name) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private void AssertParseUniqueConstraintRule( string serialized, string name )
		 {
			  // GIVEN
			  long ruleId = 1;
			  int propertyKey = 3;
			  int labelId = 55;
			  long ownedIndexId = 2;
			  UniquenessConstraintDescriptor constraint = ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyKey );
			  sbyte[] bytes = DecodeBase64( serialized );

			  // WHEN
			  ConstraintRule deserialized = AssertConstraintRule( SchemaRuleSerialization.Deserialize( ruleId, ByteBuffer.wrap( bytes ) ) );

			  // THEN
			  assertThat( deserialized.Id, equalTo( ruleId ) );
			  assertThat( deserialized.ConstraintDescriptor, equalTo( constraint ) );
			  assertThat( deserialized.Schema(), equalTo(constraint.Schema()) );
			  assertThat( deserialized.OwnedIndex, equalTo( ownedIndexId ) );
			  assertThat( deserialized.Name, @is( name ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertParseNodeKeyConstraintRule(String serialized, String name) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private void AssertParseNodeKeyConstraintRule( string serialized, string name )
		 {
			  // GIVEN
			  long ruleId = 1;
			  int propertyKey = 3;
			  int labelId = 55;
			  long ownedIndexId = 2;
			  NodeKeyConstraintDescriptor constraint = ConstraintDescriptorFactory.nodeKeyForLabel( labelId, propertyKey );
			  sbyte[] bytes = DecodeBase64( serialized );

			  // WHEN
			  ConstraintRule deserialized = AssertConstraintRule( SchemaRuleSerialization.Deserialize( ruleId, ByteBuffer.wrap( bytes ) ) );

			  // THEN
			  assertThat( deserialized.Id, equalTo( ruleId ) );
			  assertThat( deserialized.ConstraintDescriptor, equalTo( constraint ) );
			  assertThat( deserialized.Schema(), equalTo(constraint.Schema()) );
			  assertThat( deserialized.OwnedIndex, equalTo( ownedIndexId ) );
			  assertThat( deserialized.Name, @is( name ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertParseNodePropertyExistsRule(String serialized, String name) throws Exception
		 private void AssertParseNodePropertyExistsRule( string serialized, string name )
		 {
			  // GIVEN
			  long ruleId = 87;
			  int propertyKey = 51;
			  int labelId = 45;
			  ConstraintDescriptor constraint = ConstraintDescriptorFactory.existsForLabel( labelId, propertyKey );
			  sbyte[] bytes = DecodeBase64( serialized );

			  // WHEN
			  ConstraintRule deserialized = AssertConstraintRule( SchemaRuleSerialization.Deserialize( ruleId, ByteBuffer.wrap( bytes ) ) );

			  // THEN
			  assertThat( deserialized.Id, equalTo( ruleId ) );
			  assertThat( deserialized.ConstraintDescriptor, equalTo( constraint ) );
			  assertThat( deserialized.Schema(), equalTo(constraint.Schema()) );
			  assertException( deserialized.getOwnedIndex, typeof( System.InvalidOperationException ) );
			  assertThat( deserialized.Name, @is( name ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertParseRelationshipPropertyExistsRule(String serialized, String name) throws Exception
		 private void AssertParseRelationshipPropertyExistsRule( string serialized, string name )
		 {
			  // GIVEN
			  long ruleId = 51;
			  int propertyKey = 6119;
			  int relTypeId = 8512;
			  ConstraintDescriptor constraint = ConstraintDescriptorFactory.existsForRelType( relTypeId, propertyKey );
			  sbyte[] bytes = DecodeBase64( serialized );

			  // WHEN
			  ConstraintRule deserialized = AssertConstraintRule( SchemaRuleSerialization.Deserialize( ruleId, ByteBuffer.wrap( bytes ) ) );

			  // THEN
			  assertThat( deserialized.Id, equalTo( ruleId ) );
			  assertThat( deserialized.ConstraintDescriptor, equalTo( constraint ) );
			  assertThat( deserialized.Schema(), equalTo(constraint.Schema()) );
			  assertException( deserialized.getOwnedIndex, typeof( System.InvalidOperationException ) );
			  assertThat( deserialized.Name, @is( name ) );
		 }

		 // HELPERS

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSerializeAndDeserializeIndexRule(org.neo4j.storageengine.api.schema.StoreIndexDescriptor indexRule) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private void AssertSerializeAndDeserializeIndexRule( StoreIndexDescriptor indexRule )
		 {
			  StoreIndexDescriptor deserialized = AssertIndexRule( SerialiseAndDeserialise( indexRule ) );

			  assertThat( deserialized.Id, equalTo( indexRule.Id ) );
			  assertThat( deserialized, equalTo( indexRule ) );
			  assertThat( deserialized.Schema(), equalTo(indexRule.Schema()) );
			  assertThat( deserialized.ProviderDescriptor(), equalTo(indexRule.ProviderDescriptor()) );
		 }

		 private StoreIndexDescriptor AssertIndexRule( SchemaRule schemaRule )
		 {
			  if ( !( schemaRule is StoreIndexDescriptor ) )
			  {
					fail( "Expected IndexRule, but got " + schemaRule.GetType().Name );
			  }
			  return ( StoreIndexDescriptor )schemaRule;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSerializeAndDeserializeConstraintRule(ConstraintRule constraintRule) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private void AssertSerializeAndDeserializeConstraintRule( ConstraintRule constraintRule )
		 {
			  ConstraintRule deserialized = AssertConstraintRule( SerialiseAndDeserialise( constraintRule ) );

			  assertThat( deserialized.Id, equalTo( constraintRule.Id ) );
			  assertThat( deserialized.ConstraintDescriptor, equalTo( constraintRule.ConstraintDescriptor ) );
			  assertThat( deserialized.Schema(), equalTo(constraintRule.Schema()) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.storageengine.api.schema.SchemaRule serialiseAndDeserialise(ConstraintRule constraintRule) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private SchemaRule SerialiseAndDeserialise( ConstraintRule constraintRule )
		 {
			  ByteBuffer buffer = ByteBuffer.wrap( SchemaRuleSerialization.Serialize( constraintRule ) );
			  return SchemaRuleSerialization.Deserialize( constraintRule.Id, buffer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.storageengine.api.schema.SchemaRule serialiseAndDeserialise(org.neo4j.storageengine.api.schema.StoreIndexDescriptor indexRule) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private SchemaRule SerialiseAndDeserialise( StoreIndexDescriptor indexRule )
		 {
			  ByteBuffer buffer = ByteBuffer.wrap( SchemaRuleSerialization.Serialize( indexRule ) );
			  return SchemaRuleSerialization.Deserialize( indexRule.Id, buffer );
		 }

		 private ConstraintRule AssertConstraintRule( SchemaRule schemaRule )
		 {
			  if ( !( schemaRule is ConstraintRule ) )
			  {
					fail( "Expected ConstraintRule, but got " + schemaRule.GetType().Name );
			  }
			  return ( ConstraintRule )schemaRule;
		 }

		 private void AssertCorrectLength( StoreIndexDescriptor indexRule )
		 {
			  // GIVEN
			  ByteBuffer buffer = ByteBuffer.wrap( SchemaRuleSerialization.Serialize( indexRule ) );

			  // THEN
			  assertThat( SchemaRuleSerialization.LengthOf( indexRule ), equalTo( buffer.capacity() ) );
		 }

		 private void AssertCorrectLength( ConstraintRule constraintRule )
		 {
			  // GIVEN
			  ByteBuffer buffer = ByteBuffer.wrap( SchemaRuleSerialization.Serialize( constraintRule ) );

			  // THEN
			  assertThat( SchemaRuleSerialization.LengthOf( constraintRule ), equalTo( buffer.capacity() ) );
		 }

		 private sbyte[] DecodeBase64( string serialized )
		 {
			  return Base64.Decoder.decode( serialized );
		 }

		 private string EncodeBase64( sbyte[] bytes )
		 {
			  return Base64.Encoder.encodeToString( bytes );
		 }

		 /// <summary>
		 /// Used to append a null-byte to the end of the base64 input and return the resulting base64 output.
		 /// The reason we need this, is because the rule names are null-terminated strings at the end of the encoded
		 /// schema rules.
		 /// By appending a null-byte, we effectively an empty string as the rule name. However, this is not really an
		 /// allowed rule name, so when we deserialise these rules, we should get the generated rule name back.
		 /// This can potentially be used in the future in case we don't want to give a rule a name, but still want to put
		 /// fields after where the name would be.
		 /// In that case, a single null-byte would suffice to indicate that the name field is (almost) not there.
		 /// </summary>
		 private string AddNullByte( string input )
		 {
			  sbyte[] inputBytes = DecodeBase64( input );
			  sbyte[] outputBytes = Arrays.copyOf( inputBytes, inputBytes.Length + 1 );
			  return EncodeBase64( outputBytes );
		 }

		 private string AddNullByte( int nullByteCountToAdd, string input )
		 {
			  if ( nullByteCountToAdd < 1 )
			  {
					return input;
			  }
			  if ( nullByteCountToAdd == 1 )
			  {
					return AddNullByte( input );
			  }
			  return AddNullByte( AddNullByte( nullByteCountToAdd - 1, input ) );
		 }
	}

}
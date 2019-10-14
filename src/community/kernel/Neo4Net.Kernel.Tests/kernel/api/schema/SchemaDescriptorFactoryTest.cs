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
namespace Neo4Net.Kernel.api.schema
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaTestUtil.assertArray;

	public class SchemaDescriptorFactoryTest
	{

		 private const int REL_TYPE_ID = 0;
		 private const int LABEL_ID = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateLabelDescriptors()
		 public virtual void ShouldCreateLabelDescriptors()
		 {
			  LabelSchemaDescriptor labelDesc;
			  labelDesc = SchemaDescriptorFactory.ForLabel( LABEL_ID, 1 );
			  assertThat( labelDesc.LabelId, equalTo( LABEL_ID ) );
			  assertArray( labelDesc.PropertyIds, 1 );

			  labelDesc = SchemaDescriptorFactory.ForLabel( LABEL_ID, 1, 2, 3 );
			  assertThat( labelDesc.LabelId, equalTo( LABEL_ID ) );
			  SchemaTestUtil.AssertArray( labelDesc.PropertyIds, 1, 2, 3 );

			  labelDesc = SchemaDescriptorFactory.ForLabel( LABEL_ID );
			  assertThat( labelDesc.LabelId, equalTo( LABEL_ID ) );
			  SchemaTestUtil.AssertArray( labelDesc.PropertyIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRelTypeDescriptors()
		 public virtual void ShouldCreateRelTypeDescriptors()
		 {
			  RelationTypeSchemaDescriptor relTypeDesc;
			  relTypeDesc = SchemaDescriptorFactory.ForRelType( REL_TYPE_ID, 1 );
			  assertThat( relTypeDesc.RelTypeId, equalTo( REL_TYPE_ID ) );
			  assertArray( relTypeDesc.PropertyIds, 1 );

			  relTypeDesc = SchemaDescriptorFactory.ForRelType( REL_TYPE_ID, 1, 2, 3 );
			  assertThat( relTypeDesc.RelTypeId, equalTo( REL_TYPE_ID ) );
			  SchemaTestUtil.AssertArray( relTypeDesc.PropertyIds, 1, 2, 3 );

			  relTypeDesc = SchemaDescriptorFactory.ForRelType( REL_TYPE_ID );
			  assertThat( relTypeDesc.RelTypeId, equalTo( REL_TYPE_ID ) );
			  SchemaTestUtil.AssertArray( relTypeDesc.PropertyIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateEqualLabels()
		 public virtual void ShouldCreateEqualLabels()
		 {
			  LabelSchemaDescriptor desc1 = SchemaDescriptorFactory.ForLabel( LABEL_ID, 1 );
			  LabelSchemaDescriptor desc2 = SchemaDescriptorFactory.ForLabel( LABEL_ID, 1 );
			  SchemaTestUtil.AssertEquality( desc1, desc2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateEqualRelTypes()
		 public virtual void ShouldCreateEqualRelTypes()
		 {
			  RelationTypeSchemaDescriptor desc1 = SchemaDescriptorFactory.ForRelType( REL_TYPE_ID, 1 );
			  RelationTypeSchemaDescriptor desc2 = SchemaDescriptorFactory.ForRelType( REL_TYPE_ID, 1 );
			  SchemaTestUtil.AssertEquality( desc1, desc2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceUserDescriptions()
		 public virtual void ShouldGiveNiceUserDescriptions()
		 {
			  assertThat( SchemaDescriptorFactory.ForLabel( 1, 2 ).userDescription( SchemaTestUtil.simpleNameLookup ), equalTo( ":Label1(property2)" ) );
			  assertThat( SchemaDescriptorFactory.ForRelType( 1, 3 ).userDescription( SchemaTestUtil.simpleNameLookup ), equalTo( "-[:RelType1(property3)]-" ) );
		 }
	}

}
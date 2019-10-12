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
namespace Org.Neo4j.Kernel.api.schema.constraints
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaTestUtil.assertEquality;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaTestUtil.simpleNameLookup;

	public class ConstraintDescriptorFactoryTest
	{
		 private const int LABEL_ID = 0;
		 private const int REL_TYPE_ID = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExistsConstraintDescriptors()
		 public virtual void ShouldCreateExistsConstraintDescriptors()
		 {
			  ConstraintDescriptor desc;

			  desc = ConstraintDescriptorFactory.ExistsForLabel( LABEL_ID, 1 );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.EXISTS) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );

			  desc = ConstraintDescriptorFactory.ExistsForRelType( REL_TYPE_ID, 1 );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.EXISTS) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forRelType(REL_TYPE_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniqueConstraintDescriptors()
		 public virtual void ShouldCreateUniqueConstraintDescriptors()
		 {
			  ConstraintDescriptor desc;

			  desc = ConstraintDescriptorFactory.UniqueForLabel( LABEL_ID, 1 );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.UNIQUE) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodeKeyConstraintDescriptors()
		 public virtual void ShouldCreateNodeKeyConstraintDescriptors()
		 {
			  ConstraintDescriptor desc;

			  desc = ConstraintDescriptorFactory.NodeKeyForLabel( LABEL_ID, 1 );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.UNIQUE_EXISTS) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateConstraintDescriptorsFromSchema()
		 public virtual void ShouldCreateConstraintDescriptorsFromSchema()
		 {
			  ConstraintDescriptor desc;

			  desc = ConstraintDescriptorFactory.UniqueForSchema( SchemaDescriptorFactory.forLabel( LABEL_ID, 1 ) );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.UNIQUE) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );

			  desc = ConstraintDescriptorFactory.NodeKeyForSchema( SchemaDescriptorFactory.forLabel( LABEL_ID, 1 ) );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.UNIQUE_EXISTS) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );

			  desc = ConstraintDescriptorFactory.ExistsForSchema( SchemaDescriptorFactory.forRelType( REL_TYPE_ID, 1 ) );
			  assertThat( desc.Type(), equalTo(ConstraintDescriptor.Type.EXISTS) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forRelType(REL_TYPE_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateEqualDescriptors()
		 public virtual void ShouldCreateEqualDescriptors()
		 {
			  ConstraintDescriptor desc1;
			  ConstraintDescriptor desc2;

			  desc1 = ConstraintDescriptorFactory.UniqueForLabel( LABEL_ID, 1 );
			  desc2 = ConstraintDescriptorFactory.UniqueForLabel( LABEL_ID, 1 );
			  assertEquality( desc1, desc2 );

			  desc1 = ConstraintDescriptorFactory.ExistsForLabel( LABEL_ID, 1 );
			  desc2 = ConstraintDescriptorFactory.ExistsForLabel( LABEL_ID, 1 );
			  assertEquality( desc1, desc2 );

			  desc1 = ConstraintDescriptorFactory.ExistsForRelType( LABEL_ID, 1 );
			  desc2 = ConstraintDescriptorFactory.ExistsForRelType( LABEL_ID, 1 );
			  assertEquality( desc1, desc2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceUserDescriptions()
		 public virtual void ShouldGiveNiceUserDescriptions()
		 {
			  assertThat( ConstraintDescriptorFactory.ExistsForLabel( 1, 2 ).userDescription( simpleNameLookup ), equalTo( "Constraint( EXISTS, :Label1(property2) )" ) );
			  assertThat( ConstraintDescriptorFactory.ExistsForRelType( 1, 3 ).userDescription( simpleNameLookup ), equalTo( "Constraint( EXISTS, -[:RelType1(property3)]- )" ) );
			  assertThat( ConstraintDescriptorFactory.UniqueForLabel( 2, 4 ).userDescription( simpleNameLookup ), equalTo( "Constraint( UNIQUE, :Label2(property4) )" ) );
		 }
	}

}
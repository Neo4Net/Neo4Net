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
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using Test = org.junit.Test;

	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using Neo4Net.Helpers.Collection;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Pair.of;

	public class DbStructureCollectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void collectsDbStructure()
		 public virtual void CollectsDbStructure()
		 {
			  // GIVEN
			  DbStructureCollector collector = new DbStructureCollector();
			  collector.VisitLabel( 1, "Person" );
			  collector.VisitLabel( 2, "City" );
			  collector.VisitPropertyKey( 1, "name" );
			  collector.VisitPropertyKey( 2, "income" );
			  collector.VisitRelationshipType( 1, "LIVES_IN" );
			  collector.VisitRelationshipType( 2, "FRIEND" );
			  collector.VisitIndex( TestIndexDescriptorFactory.uniqueForLabel( 1, 1 ), ":Person(name)", 1.0d, 1L );
			  collector.VisitUniqueConstraint( ConstraintDescriptorFactory.uniqueForLabel( 2, 1 ), ":City(name)" );
			  collector.VisitNodeKeyConstraint( ConstraintDescriptorFactory.nodeKeyForLabel( 2, 1 ), ":City(name)" );
			  collector.VisitIndex( TestIndexDescriptorFactory.forLabel( 2, 2 ), ":City(income)", 0.2d, 1L );
			  collector.VisitAllNodesCount( 50 );
			  collector.VisitNodeCount( 1, "Person", 20 );
			  collector.VisitNodeCount( 2, "City", 30 );
			  collector.VisitRelCount( 1, 2, -1, "(:Person)-[:FRIEND]->()", 500 );

			  // WHEN
			  DbStructureLookup lookup = collector.Lookup();

			  // THEN
			  assertEquals( asList( of( 1, "Person" ), of( 2, "City" ) ), Iterators.asList( lookup.Labels() ) );
			  assertEquals( asList( of( 1, "name" ), of( 2, "income" ) ), Iterators.asList( lookup.Properties() ) );
			  assertEquals( asList( of( 1, "LIVES_IN" ), of( 2, "FRIEND" ) ), Iterators.asList( lookup.RelationshipTypes() ) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[] { "Person" }, lookup.KnownUniqueIndices().next().first() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "name" }, lookup.KnownUniqueIndices().next().other() );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( asList( "City" ), Iterators.asList( Iterators.map( Pair::first, lookup.KnownNodeKeyConstraints() ) ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "name" }, lookup.KnownNodeKeyConstraints().next().other() );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( asList( "City" ), Iterators.asList( Iterators.map( Pair::first, lookup.KnownUniqueConstraints() ) ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "name" }, lookup.KnownUniqueConstraints().next().other() );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( new string[] { "City" }, lookup.KnownIndices().next().first() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "income" }, lookup.KnownIndices().next().other() );

			  assertEquals( 50, lookup.NodesAllCardinality() );
			  assertEquals( 20, lookup.NodesWithLabelCardinality( 1 ) );
			  assertEquals( 30, lookup.NodesWithLabelCardinality( 2 ) );
			  assertEquals( 500, lookup.CardinalityByLabelsAndRelationshipType( 1, 2, -1 ) );
			  assertEquals( 1.0d, lookup.IndexUniqueValueSelectivity( 1, 1 ), 0.01d );
			  assertEquals( 0.2d, lookup.IndexUniqueValueSelectivity( 2, 2 ), 0.01d );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void collectsCompositeDbStructure()
		 public virtual void CollectsCompositeDbStructure()
		 {
			  // GIVEN
			  DbStructureCollector collector = new DbStructureCollector();
			  collector.VisitLabel( 1, "Person" );
			  collector.VisitLabel( 2, "City" );
			  collector.VisitPropertyKey( 1, "name" );
			  collector.VisitPropertyKey( 2, "income" );
			  collector.VisitPropertyKey( 3, "lastName" );
			  collector.VisitPropertyKey( 4, "tax" );
			  collector.VisitPropertyKey( 5, "area" );
			  collector.VisitRelationshipType( 1, "LIVES_IN" );
			  collector.VisitRelationshipType( 2, "FRIEND" );
			  collector.VisitIndex( TestIndexDescriptorFactory.uniqueForLabel( 1, 1, 3 ), ":Person(name, lastName)", 1.0d, 1L );
			  collector.VisitUniqueConstraint( ConstraintDescriptorFactory.uniqueForLabel( 2, 1, 5 ), ":City(name, area)" );
			  collector.VisitIndex( TestIndexDescriptorFactory.forLabel( 2, 2, 4 ), ":City(income, tax)", 0.2d, 1L );
			  collector.VisitAllNodesCount( 50 );
			  collector.VisitNodeCount( 1, "Person", 20 );
			  collector.VisitNodeCount( 2, "City", 30 );
			  collector.VisitRelCount( 1, 2, -1, "(:Person)-[:FRIEND]->()", 500 );

			  // WHEN
			  DbStructureLookup lookup = collector.Lookup();

			  // THEN
			  assertEquals( asList( of( 1, "Person" ), of( 2, "City" ) ), Iterators.asList( lookup.Labels() ) );
			  assertEquals( asList( of( 1, "name" ), of( 2, "income" ), of( 3, "lastName" ), of( 4, "tax" ), of( 5, "area" ) ), Iterators.asList( lookup.Properties() ) );
			  assertEquals( asList( of( 1, "LIVES_IN" ), of( 2, "FRIEND" ) ), Iterators.asList( lookup.RelationshipTypes() ) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[] { "Person" }, lookup.KnownUniqueIndices().next().first() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "name", "lastName" }, lookup.KnownUniqueIndices().next().other() );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( asList( "City" ), Iterators.asList( Iterators.map( Pair::first, lookup.KnownUniqueConstraints() ) ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "name", "area" }, lookup.KnownUniqueConstraints().next().other() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( new string[] { "City" }, lookup.KnownIndices().next().first() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertArrayEquals( new string[]{ "income", "tax" }, lookup.KnownIndices().next().other() );

			  assertEquals( 50, lookup.NodesAllCardinality() );
			  assertEquals( 20, lookup.NodesWithLabelCardinality( 1 ) );
			  assertEquals( 30, lookup.NodesWithLabelCardinality( 2 ) );
			  assertEquals( 500, lookup.CardinalityByLabelsAndRelationshipType( 1, 2, -1 ) );
			  assertEquals( 1.0d, lookup.IndexUniqueValueSelectivity( 1, 1, 3 ), 0.01d );
			  assertEquals( 0.2d, lookup.IndexUniqueValueSelectivity( 2, 2, 4 ), 0.01d );
		 }
	}

}
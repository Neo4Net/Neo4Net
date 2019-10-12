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
namespace Org.Neo4j.Tooling.procedure.procedures.valid
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Name = Org.Neo4j.Procedure.Name;
	using PerformsWrites = Org.Neo4j.Procedure.PerformsWrites;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.DBMS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.SCHEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.WRITE;

	public class Procedures
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.LongWrapper> theAnswer()
		 public virtual Stream<Records.LongWrapper> TheAnswer()
		 {
			  return Stream.of( new Records.LongWrapper( 42L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput00()
		 public virtual void SimpleInput00()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput01(@Name("foo") String input)
		 public virtual void SimpleInput01( string input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput02(@Name("foo") long input)
		 public virtual void SimpleInput02( long input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput03(@Name("foo") System.Nullable<long> input)
		 public virtual void SimpleInput03( long? input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput04(@Name("foo") Number input)
		 public virtual void SimpleInput04( Number input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput05(@Name("foo") System.Nullable<bool> input)
		 public virtual void SimpleInput05( bool? input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput06(@Name("foo") boolean input)
		 public virtual void SimpleInput06( bool input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput07(@Name("foo") Object input)
		 public virtual void SimpleInput07( object input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput08(@Name("foo") org.neo4j.graphdb.Node input)
		 public virtual void SimpleInput08( Node input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput09(@Name("foo") org.neo4j.graphdb.Path input)
		 public virtual void SimpleInput09( Path input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void simpleInput10(@Name("foo") org.neo4j.graphdb.Relationship input)
		 public virtual void SimpleInput10( Relationship input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput11(@Name("foo") String input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput11( string input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput12(@Name("foo") long input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput12( long input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput13(@Name("foo") System.Nullable<long> input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput13( long? input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput14(@Name("foo") Number input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput14( Number input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput15(@Name("foo") System.Nullable<bool> input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput15( bool? input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput16(@Name("foo") boolean input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput16( bool input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput17(@Name("foo") Object input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput17( object input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput18(@Name("foo") org.neo4j.graphdb.Node input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput18( Node input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput19(@Name("foo") org.neo4j.graphdb.Path input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput19( Path input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput20(@Name("foo") org.neo4j.graphdb.Relationship input)
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput20( Relationship input )
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.SimpleTypesWrapper> simpleInput21()
		 public virtual Stream<Records.SimpleTypesWrapper> SimpleInput21()
		 {
			  return Stream.of( new Records.SimpleTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void genericInput01(@Name("foo") java.util.List<String> input)
		 public virtual void GenericInput01( IList<string> input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void genericInput02(@Name("foo") java.util.List<java.util.List<org.neo4j.graphdb.Node>> input)
		 public virtual void GenericInput02( IList<IList<Node>> input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void genericInput03(@Name("foo") java.util.Map<String,java.util.List<org.neo4j.graphdb.Node>> input)
		 public virtual void GenericInput03( IDictionary<string, IList<Node>> input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void genericInput04(@Name("foo") java.util.Map<String,Object> input)
		 public virtual void GenericInput04( IDictionary<string, object> input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void genericInput05(@Name("foo") java.util.Map<String,java.util.List<java.util.List<java.util.Map<String,java.util.Map<String,java.util.List<org.neo4j.graphdb.Path>>>>>> input)
		 public virtual void GenericInput05( IDictionary<string, IList<IList<IDictionary<string, IDictionary<string, IList<Path>>>>>> input )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.GenericTypesWrapper> genericInput06(@Name("foo") java.util.List<String> input)
		 public virtual Stream<Records.GenericTypesWrapper> GenericInput06( IList<string> input )
		 {
			  return Stream.of( new Records.GenericTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.GenericTypesWrapper> genericInput07(@Name("foo") java.util.List<java.util.List<org.neo4j.graphdb.Node>> input)
		 public virtual Stream<Records.GenericTypesWrapper> GenericInput07( IList<IList<Node>> input )
		 {
			  return Stream.of( new Records.GenericTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.GenericTypesWrapper> genericInput08(@Name("foo") java.util.Map<String,java.util.List<org.neo4j.graphdb.Node>> input)
		 public virtual Stream<Records.GenericTypesWrapper> GenericInput08( IDictionary<string, IList<Node>> input )
		 {
			  return Stream.of( new Records.GenericTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.GenericTypesWrapper> genericInput09(@Name("foo") java.util.Map<String,Object> input)
		 public virtual Stream<Records.GenericTypesWrapper> GenericInput09( IDictionary<string, object> input )
		 {
			  return Stream.of( new Records.GenericTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Records.GenericTypesWrapper> genericInput10(@Name("foo") java.util.Map<String,java.util.List<java.util.List<java.util.Map<String,java.util.Map<String,java.util.List<org.neo4j.graphdb.Path>>>>>> input)
		 public virtual Stream<Records.GenericTypesWrapper> GenericInput10( IDictionary<string, IList<IList<IDictionary<string, IDictionary<string, IList<Path>>>>>> input )
		 {
			  return Stream.of( new Records.GenericTypesWrapper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure @PerformsWrites public void performsWrites()
		 public virtual void PerformsWrites()
		 {
		 }

		 [Procedure(mode : DEFAULT)]
		 public virtual void DefaultMode()
		 {
		 }

		 [Procedure(mode : READ)]
		 public virtual void ReadMode()
		 {
		 }

		 [Procedure(mode : WRITE)]
		 public virtual void WriteMode()
		 {
		 }

		 [Procedure(mode : SCHEMA)]
		 public virtual void SchemaMode()
		 {
		 }

		 [Procedure(mode : DBMS)]
		 public virtual void DbmsMode()
		 {
		 }
	}

}
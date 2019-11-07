using System.Collections.Generic;

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
namespace Neo4Net.Tooling.procedure.procedures.valid
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Name = Neo4Net.Procedure.Name;
	using UserFunction = Neo4Net.Procedure.UserFunction;

	public class UserFunctions
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput00()
		 public virtual string SimpleInput00()
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput01(@Name("foo") String input)
		 public virtual string SimpleInput01( string input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput02(@Name("foo") long input)
		 public virtual string SimpleInput02( long input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput03(@Name("foo") System.Nullable<long> input)
		 public virtual string SimpleInput03( long? input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput04(@Name("foo") Number input)
		 public virtual string SimpleInput04( Number input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput05(@Name("foo") System.Nullable<bool> input)
		 public virtual string SimpleInput05( bool? input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput06(@Name("foo") boolean input)
		 public virtual string SimpleInput06( bool input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput07(@Name("foo") Object input)
		 public virtual string SimpleInput07( object input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput08(@Name("foo") Neo4Net.graphdb.Node input)
		 public virtual string SimpleInput08( Node input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput09(@Name("foo") Neo4Net.graphdb.Path input)
		 public virtual string SimpleInput09( Path input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String simpleInput10(@Name("foo") Neo4Net.graphdb.Relationship input)
		 public virtual string SimpleInput10( Relationship input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String genericInput01(@Name("foo") java.util.List<String> input)
		 public virtual string GenericInput01( IList<string> input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String genericInput02(@Name("foo") java.util.List<java.util.List<Neo4Net.graphdb.Node>> input)
		 public virtual string GenericInput02( IList<IList<Node>> input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String genericInput03(@Name("foo") java.util.Map<String,java.util.List<Neo4Net.graphdb.Node>> input)
		 public virtual string GenericInput03( IDictionary<string, IList<Node>> input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String genericInput04(@Name("foo") java.util.Map<String,Object> input)
		 public virtual string GenericInput04( IDictionary<string, object> input )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String genericInput05(@Name("foo") java.util.Map<String,java.util.List<java.util.List<java.util.Map<String,java.util.Map<String,java.util.List<Neo4Net.graphdb.Path>>>>>> input)
		 public virtual string GenericInput05( IDictionary<string, IList<IList<IDictionary<string, IDictionary<string, IList<Path>>>>>> input )
		 {
			  return "42";
		 }

	}

}
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Tooling.procedure.procedures.invalid.bad_proc_input_type
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Name = Neo4Net.Procedure.Name;
	using UserFunction = Neo4Net.Procedure.UserFunction;

	public class BadGenericInputUserFunction
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String doSomething(@Name("test") java.util.List<java.util.List<java.util.Map<String,Thread>>> unsupportedType)
		 public virtual string DoSomething( IList<IList<IDictionary<string, Thread>>> unsupportedType )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String doSomething2(@Name("test") java.util.Map<String,java.util.List<java.util.concurrent.ExecutorService>> unsupportedType)
		 public virtual string DoSomething2( IDictionary<string, IList<ExecutorService>> unsupportedType )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String doSomething3(@Name("test") java.util.Map unsupportedType)
		 public virtual string DoSomething3( System.Collections.IDictionary unsupportedType )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works1(@Name("test") java.util.List<String> supported)
		 public virtual string Works1( IList<string> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works2(@Name("test") java.util.List<java.util.List<Object>> supported)
		 public virtual string Works2( IList<IList<object>> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works3(@Name("test") java.util.Map<String,Object> supported)
		 public virtual string Works3( IDictionary<string, object> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works4(@Name("test") java.util.List<java.util.List<java.util.List<java.util.Map<String,Object>>>> supported)
		 public virtual string Works4( IList<IList<IList<IDictionary<string, object>>>> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works5(@Name("test") java.util.List<java.util.List<java.util.List<Neo4Net.graphdb.Path>>> supported)
		 public virtual string Works5( IList<IList<IList<Path>>> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works6(@Name("test") java.util.List<Neo4Net.graphdb.Node> supported)
		 public virtual string Works6( IList<Node> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works7(@Name("test") java.util.List<java.util.List<Neo4Net.graphdb.Relationship>> supported)
		 public virtual string Works7( IList<IList<Relationship>> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works8(@Name("test") java.util.Map<String,java.util.List<java.util.List<Neo4Net.graphdb.Relationship>>> supported)
		 public virtual string Works8( IDictionary<string, IList<IList<Relationship>>> supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works9(@Name("test") java.util.Map<String,java.util.Map<String,java.util.List<Neo4Net.graphdb.Node>>> supported)
		 public virtual string Works9( IDictionary<string, IDictionary<string, IList<Node>>> supported )
		 {
			  return "42";
		 }
	}

}
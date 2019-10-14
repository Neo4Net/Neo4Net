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

	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

	public class BadGenericInputSproc
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void doSomething(@Name("test") java.util.List<java.util.List<java.util.Map<String,Thread>>> unsupportedType)
		 public virtual void DoSomething( IList<IList<IDictionary<string, Thread>>> unsupportedType )
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void doSomething2(@Name("test") java.util.Map<String,java.util.List<java.util.concurrent.ExecutorService>> unsupportedType)
		 public virtual void DoSomething2( IDictionary<string, IList<ExecutorService>> unsupportedType )
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void doSomething3(@Name("test") java.util.Map unsupportedType)
		 public virtual void DoSomething3( System.Collections.IDictionary unsupportedType )
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works1(@Name("test") java.util.List<String> supported)
		 public virtual void Works1( IList<string> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works2(@Name("test") java.util.List<java.util.List<Object>> supported)
		 public virtual void Works2( IList<IList<object>> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works3(@Name("test") java.util.Map<String,Object> supported)
		 public virtual void Works3( IDictionary<string, object> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works4(@Name("test") java.util.List<java.util.List<java.util.List<java.util.Map<String,Object>>>> supported)
		 public virtual void Works4( IList<IList<IList<IDictionary<string, object>>>> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works5(@Name("test") java.util.List<java.util.List<java.util.List<org.neo4j.graphdb.Path>>> supported)
		 public virtual void Works5( IList<IList<IList<Path>>> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works6(@Name("test") java.util.List<org.neo4j.graphdb.Node> supported)
		 public virtual void Works6( IList<Node> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works7(@Name("test") java.util.List<java.util.List<org.neo4j.graphdb.Relationship>> supported)
		 public virtual void Works7( IList<IList<Relationship>> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works8(@Name("test") java.util.Map<String,java.util.List<java.util.List<org.neo4j.graphdb.Relationship>>> supported)
		 public virtual void Works8( IDictionary<string, IList<IList<Relationship>>> supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works9(@Name("test") java.util.Map<String,java.util.Map<String,java.util.List<org.neo4j.graphdb.Node>>> supported)
		 public virtual void Works9( IDictionary<string, IDictionary<string, IList<Node>>> supported )
		 {
		 }
	}

}
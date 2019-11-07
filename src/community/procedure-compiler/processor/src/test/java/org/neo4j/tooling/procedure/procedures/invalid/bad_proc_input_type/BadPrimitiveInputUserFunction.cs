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

	public class BadPrimitiveInputUserFunction
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String doSomething(@Name("test") short unsupportedType)
		 public virtual string DoSomething( short unsupportedType )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works01(@Name("test") String supported)
		 public virtual string Works01( string supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works02(@Name("test") System.Nullable<long> supported)
		 public virtual string Works02( long? supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works03(@Name("test") long supported)
		 public virtual string Works03( long supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works04(@Name("test") System.Nullable<double> supported)
		 public virtual string Works04( double? supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works05(@Name("test") double supported)
		 public virtual string Works05( double supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works06(@Name("test") Number supported)
		 public virtual string Works06( Number supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works07(@Name("test") System.Nullable<bool> supported)
		 public virtual string Works07( bool? supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works08(@Name("test") boolean supported)
		 public virtual string Works08( bool supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works09(@Name("test") Object supported)
		 public virtual string Works09( object supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works10(@Name("test") Neo4Net.graphdb.Node supported)
		 public virtual string Works10( Node supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works11(@Name("test") Neo4Net.graphdb.Relationship supported)
		 public virtual string Works11( Relationship supported )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String works12(@Name("test") Neo4Net.graphdb.Path supported)
		 public virtual string Works12( Path supported )
		 {
			  return "42";
		 }
	}

}
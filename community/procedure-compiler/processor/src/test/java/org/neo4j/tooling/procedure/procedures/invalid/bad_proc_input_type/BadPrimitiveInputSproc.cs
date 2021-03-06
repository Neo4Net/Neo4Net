﻿/*
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
namespace Org.Neo4j.Tooling.procedure.procedures.invalid.bad_proc_input_type
{
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;

	public class BadPrimitiveInputSproc
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void doSomething(@Name("test") short unsupportedType)
		 public virtual void DoSomething( short unsupportedType )
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works01(@Name("test") String supported)
		 public virtual void Works01( string supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works02(@Name("test") System.Nullable<long> supported)
		 public virtual void Works02( long? supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works03(@Name("test") long supported)
		 public virtual void Works03( long supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works04(@Name("test") System.Nullable<double> supported)
		 public virtual void Works04( double? supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works05(@Name("test") double supported)
		 public virtual void Works05( double supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works06(@Name("test") Number supported)
		 public virtual void Works06( Number supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works07(@Name("test") System.Nullable<bool> supported)
		 public virtual void Works07( bool? supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works08(@Name("test") boolean supported)
		 public virtual void Works08( bool supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works09(@Name("test") Object supported)
		 public virtual void Works09( object supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works10(@Name("test") org.neo4j.graphdb.Node supported)
		 public virtual void Works10( Node supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works11(@Name("test") org.neo4j.graphdb.Relationship supported)
		 public virtual void Works11( Relationship supported )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void works12(@Name("test") org.neo4j.graphdb.Path supported)
		 public virtual void Works12( Path supported )
		 {
		 }
	}

}
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
namespace Org.Neo4j.Tooling.procedure.visitors.examples
{
	using Mode = Org.Neo4j.Procedure.Mode;
	using PerformsWrites = Org.Neo4j.Procedure.PerformsWrites;
	using Procedure = Org.Neo4j.Procedure.Procedure;

	public class PerformsWriteProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites public void missingProcedureAnnotation()
		 public virtual void MissingProcedureAnnotation()
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = org.neo4j.procedure.Mode.READ) @PerformsWrites public void conflictingMode()
		 [Procedure(mode : Org.Neo4j.Procedure.Mode.READ)]
		 public virtual void ConflictingMode()
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure @PerformsWrites public void ok()
		 public virtual void Ok()
		 {

		 }
	}

}
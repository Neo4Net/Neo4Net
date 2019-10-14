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
namespace Neo4Net.Kernel.builtinprocs
{
	using IllegalTokenNameException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using TooManyLabelsException = Neo4Net.Internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.WRITE;

	public class TokenProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.KernelTransaction tx;
		 public KernelTransaction Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a label") @Procedure(name = "db.createLabel", mode = WRITE) public void createLabel(@Name("newLabel") String newLabel) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a label"), Procedure(name : "db.createLabel", mode : WRITE)]
		 public virtual void CreateLabel( string newLabel )
		 {

			  Tx.tokenWrite().labelGetOrCreateForName(newLabel);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a RelationshipType") @Procedure(name = "db.createRelationshipType", mode = WRITE) public void createRelationshipType(@Name("newRelationshipType") String newRelationshipType) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a RelationshipType"), Procedure(name : "db.createRelationshipType", mode : WRITE)]
		 public virtual void CreateRelationshipType( string newRelationshipType )
		 {
			  Tx.tokenWrite().relationshipTypeGetOrCreateForName(newRelationshipType);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a Property") @Procedure(name = "db.createProperty", mode = WRITE) public void createProperty(@Name("newProperty") String newProperty) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a Property"), Procedure(name : "db.createProperty", mode : WRITE)]
		 public virtual void CreateProperty( string newProperty )
		 {
			  Tx.tokenWrite().propertyKeyGetOrCreateForName(newProperty);
		 }

	}

}
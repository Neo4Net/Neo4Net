﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.coreapi.schema
{

	using Label = Neo4Net.GraphDb.Label;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;

	public class NodePropertyUniqueConstraintCreator : BaseNodeConstraintCreator
	{
		 protected internal readonly List<string> PropertyKeys = new List<string>();

		 internal NodePropertyUniqueConstraintCreator( InternalSchemaActions internalCreator, Label label, string propertyKey ) : base( internalCreator, label )
		 {
			  this.PropertyKeys.Add( propertyKey );
		 }

		 public override NodePropertyUniqueConstraintCreator AssertPropertyIsUnique( string propertyKey )
		 {
			  throw new System.NotSupportedException( "You can only create one unique constraint at a time." );
		 }

		 public override ConstraintDefinition Create()
		 {
			  AssertInUnterminatedTransaction();

			  IndexDefinitionImpl definition = new IndexDefinitionImpl( Actions, null, new Label[]{ Label }, PropertyKeys.ToArray(), true );
			  return Actions.createPropertyUniquenessConstraint( definition );
		 }
	}

}
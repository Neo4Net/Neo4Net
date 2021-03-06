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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{

	using Label = Org.Neo4j.Graphdb.Label;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.coreapi.schema.IndexDefinitionImpl.labelNameList;

	internal abstract class NodeConstraintDefinition : MultiPropertyConstraintDefinition
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly Label LabelConflict;

		 protected internal NodeConstraintDefinition( InternalSchemaActions actions, Label label, string[] propertyKeys ) : base( actions, propertyKeys )
		 {
			  this.LabelConflict = requireNonNull( label );
		 }

		 protected internal NodeConstraintDefinition( InternalSchemaActions actions, IndexDefinition indexDefinition ) : base( actions, indexDefinition )
		 {
			  if ( indexDefinition.MultiTokenIndex )
			  {
					throw new System.ArgumentException( "Node constraints do not support multi-token definitions. That is, they cannot apply to more than one label, " + "but an attempt was made to create a node constraint on the following labels: " + labelNameList( indexDefinition.Labels, "", "." ) );
			  }
			  this.LabelConflict = single( indexDefinition.Labels );
		 }

		 public virtual Label Label
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return LabelConflict;
			 }
		 }

		 public virtual RelationshipType RelationshipType
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  throw new System.InvalidOperationException( "Constraint is associated with nodes" );
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  NodeConstraintDefinition that = ( NodeConstraintDefinition ) o;
			  return LabelConflict.name().Equals(that.LabelConflict.name()) && Arrays.Equals(PropertyKeysConflict, that.PropertyKeysConflict);
		 }

		 protected internal virtual string PropertyText()
		 {
			  string nodeVariable = LabelConflict.name().ToLower();
			  if ( PropertyKeysConflict.Length == 1 )
			  {
					return nodeVariable + "." + PropertyKeysConflict[0];
			  }
			  else
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					return "(" + java.util.propertyKeys.Select( p => nodeVariable + "." + p ).collect( Collectors.joining( "," ) ) + ")";
			  }
		 }

		 public override int GetHashCode()
		 {
			  return 31 * LabelConflict.name().GetHashCode() + Arrays.GetHashCode(PropertyKeysConflict);
		 }
	}

}
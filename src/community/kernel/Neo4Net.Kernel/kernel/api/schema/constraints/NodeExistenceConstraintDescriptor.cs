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
namespace Neo4Net.Kernel.Api.schema.constraints
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using LabelSchemaSupplier = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaSupplier;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil;

	public class NodeExistenceConstraintDescriptor : ConstraintDescriptor, LabelSchemaSupplier
	{
		 private LabelSchemaDescriptor _schema;

		 internal NodeExistenceConstraintDescriptor( LabelSchemaDescriptor schema ) : base( Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor_Type.Exists )
		 {
			  this._schema = schema;
		 }

		 public override LabelSchemaDescriptor Schema()
		 {
			  return _schema;
		 }

		 public override string PrettyPrint( TokenNameLookup tokenNameLookup )
		 {
			  string labelName = EscapeLabelOrRelTyp( tokenNameLookup.LabelGetName( _schema.LabelId ) );
			  string nodeName = labelName.ToLower();
			  string properties = SchemaUtil.niceProperties( tokenNameLookup, _schema.PropertyIds, nodeName + ".", false );

			  return string.Format( "CONSTRAINT ON ( {0}:{1} ) ASSERT exists({2})", nodeName, labelName, properties );
		 }
	}

}
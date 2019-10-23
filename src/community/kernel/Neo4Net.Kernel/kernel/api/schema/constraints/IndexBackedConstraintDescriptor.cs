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
namespace Neo4Net.Kernel.api.schema.constraints
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using LabelSchemaSupplier = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaSupplier;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.schema.SchemaUtil;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;

	public abstract class IndexBackedConstraintDescriptor : ConstraintDescriptor, LabelSchemaSupplier
	{
		 private readonly LabelSchemaDescriptor _schema;

		 internal IndexBackedConstraintDescriptor( Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor_Type type, LabelSchemaDescriptor schema ) : base( type )
		 {
			  this._schema = schema;
		 }

		 public override LabelSchemaDescriptor Schema()
		 {
			  return _schema;
		 }

		 public virtual IndexDescriptor OwnedIndexDescriptor()
		 {
			  return IndexDescriptorFactory.uniqueForSchema( _schema );
		 }

		 public override string PrettyPrint( TokenNameLookup tokenNameLookup )
		 {
			  string labelName = EscapeLabelOrRelTyp( tokenNameLookup.LabelGetName( _schema.LabelId ) );
			  string nodeName = labelName.ToLower();

			  return string.Format( "CONSTRAINT ON ( {0}:{1} ) ASSERT {2} IS {3}", nodeName, labelName, FormatProperties( _schema.PropertyIds, tokenNameLookup, nodeName ), ConstraintTypeText() );
		 }

		 protected internal abstract string ConstraintTypeText();

		 protected internal virtual string FormatProperties( int[] propertyIds, TokenNameLookup tokenNameLookup, string nodeName )
		 {
			  return SchemaUtil.niceProperties( tokenNameLookup, propertyIds, nodeName + ".", propertyIds.Length > 1 );
		 }
	}

}
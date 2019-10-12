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
namespace Org.Neo4j.Kernel.api.schema.constraints
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using LabelSchemaSupplier = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaSupplier;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;

	public abstract class IndexBackedConstraintDescriptor : ConstraintDescriptor, LabelSchemaSupplier
	{
		 private readonly LabelSchemaDescriptor _schema;

		 internal IndexBackedConstraintDescriptor( Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor_Type type, LabelSchemaDescriptor schema ) : base( type )
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
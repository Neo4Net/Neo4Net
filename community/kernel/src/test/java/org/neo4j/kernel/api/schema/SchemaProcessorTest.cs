using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.api.schema
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;


	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaProcessor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	public class SchemaProcessorTest
	{
		 private const int LABEL_ID = 0;
		 private const int REL_TYPE_ID = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCorrectDescriptorVersions()
		 public virtual void ShouldHandleCorrectDescriptorVersions()
		 {
			  IList<string> callHistory = new List<string>();
			  SchemaProcessor processor = new SchemaProcessorAnonymousInnerClass( this, callHistory );

			  DisguisedLabel().processWith(processor);
			  DisguisedLabel().processWith(processor);
			  DisguisedRelType().processWith(processor);
			  DisguisedLabel().processWith(processor);
			  DisguisedRelType().processWith(processor);
			  DisguisedRelType().processWith(processor);

			  assertThat( callHistory, Matchers.contains( "LabelSchemaDescriptor", "LabelSchemaDescriptor", "RelationTypeSchemaDescriptor", "LabelSchemaDescriptor", "RelationTypeSchemaDescriptor", "RelationTypeSchemaDescriptor" ) );
		 }

		 private class SchemaProcessorAnonymousInnerClass : SchemaProcessor
		 {
			 private readonly SchemaProcessorTest _outerInstance;

			 private IList<string> _callHistory;

			 public SchemaProcessorAnonymousInnerClass( SchemaProcessorTest outerInstance, IList<string> callHistory )
			 {
				 this.outerInstance = outerInstance;
				 this._callHistory = callHistory;
			 }

			 public void processSpecific( Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor schema )
			 {
				  _callHistory.Add( "LabelSchemaDescriptor" );
			 }

			 public void processSpecific( Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor schema )
			 {
				  _callHistory.Add( "RelationTypeSchemaDescriptor" );
			 }

			 public void processSpecific( SchemaDescriptor schemaDescriptor )
			 {
				  _callHistory.Add( "SchemaDescriptor" );
			 }
		 }

		 private SchemaDescriptor DisguisedLabel()
		 {
			  return SchemaDescriptorFactory.ForLabel( LABEL_ID, 1 );
		 }

		 private SchemaDescriptor DisguisedRelType()
		 {
			  return SchemaDescriptorFactory.ForRelType( REL_TYPE_ID, 1 );
		 }
	}

}
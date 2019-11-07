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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using MisconfiguredIndexException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.MisconfiguredIndexException;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

	public interface IIndexingProvidersService
	{
		 /// <summary>
		 /// Get the index provider descriptor for the index provider with the given name, or the
		 /// descriptor of the default index provider, if no name was given.
		 /// </summary>
		 /// <param name="providerName"> name of the wanted index provider </param>
		 IndexProviderDescriptor IndexProviderByName( string providerName );

		 /// <summary>
		 /// Validate that the given value tuple can be stored in the index associated with the given schema.
		 /// </summary>
		 /// <param name="schema"> index schema of the target index </param>
		 /// <param name="tuple"> value tuple to validate </param>
		 void ValidateBeforeCommit( SchemaDescriptor schema, Value[] tuple );

		 /// <summary>
		 /// Since indexes can now have provider-specific settings and configurations, the provider needs to have an opportunity to inspect and validate the index
		 /// descriptor before an index is created. The return descriptor is a blessed version of the given descriptor, and is what must be used for creating an
		 /// index. </summary>
		 /// <param name="index"> The descriptor of an index that we are about to create, and we wish to be blessed by its chosen index provider. </param>
		 /// <returns> The blessed index descriptor. </returns>
		 /// <exception cref="MisconfiguredIndexException"> if the provider cannot be bless the given index descriptor. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor getBlessedDescriptorFromProvider(Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor index) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.MisconfiguredIndexException;
		 IndexDescriptor GetBlessedDescriptorFromProvider( IndexDescriptor index );
	}

}
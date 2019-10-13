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

	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Contains mapping from <seealso cref="IndexProviderDescriptor"/> or provider name to <seealso cref="IndexProvider"/>.
	/// </summary>
	public interface IndexProviderMap
	{
		 /// <summary>
		 /// Looks up and returns the <seealso cref="IndexProvider"/> for the given <seealso cref="IndexProviderDescriptor"/>.
		 /// </summary>
		 /// <param name="providerDescriptor"> the descriptor identifying the <seealso cref="IndexProvider"/>. </param>
		 /// <returns> the <seealso cref="IndexProvider"/> with the given <seealso cref="IndexProviderDescriptor"/>. </returns>
		 /// <exception cref="IndexProviderNotFoundException"> if no such <seealso cref="IndexProvider"/> was found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexProvider lookup(org.neo4j.internal.kernel.api.schema.IndexProviderDescriptor providerDescriptor) throws IndexProviderNotFoundException;
		 IndexProvider Lookup( IndexProviderDescriptor providerDescriptor );

		 /// <summary>
		 /// Looks up and returns the <seealso cref="IndexProvider"/> for the given index provider name. The name is what
		 /// an <seealso cref="IndexProviderDescriptor.name()"/> call would return.
		 /// </summary>
		 /// <param name="providerDescriptorName"> the descriptor name identifying the <seealso cref="IndexProvider"/>. </param>
		 /// <returns> the <seealso cref="IndexProvider"/> with the given name. </returns>
		 /// <exception cref="IndexProviderNotFoundException"> if no such <seealso cref="IndexProvider"/> was found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexProvider lookup(String providerDescriptorName) throws IndexProviderNotFoundException;
		 IndexProvider Lookup( string providerDescriptorName );

		 /// <summary>
		 /// There's always a default <seealso cref="IndexProvider"/>, this method returns it.
		 /// </summary>
		 /// <returns> the default index provider for this instance. </returns>
		 IndexProvider DefaultProvider { get; }

		 /// <summary>
		 /// Visits all the <seealso cref="IndexProvider"/> with the visitor.
		 /// </summary>
		 /// <param name="visitor"> <seealso cref="Consumer"/> visiting all the <seealso cref="IndexProvider index providers"/> in this map. </param>
		 void Accept( System.Action<IndexProvider> visitor );

		 /// <summary>
		 /// Create a <seealso cref="CapableIndexDescriptor"/> from the given index descriptor, which includes the capabilities
		 /// that correspond to those of the index provider of the given {@code descriptor}, found in this <seealso cref="IndexProviderMap"/>.
		 /// </summary>
		 /// <returns> a CapableIndexDescriptor. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.neo4j.storageengine.api.schema.CapableIndexDescriptor withCapabilities(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor)
	//	 {
	//		  IndexProviderDescriptor providerDescriptor = descriptor.providerDescriptor();
	//		  IndexCapability capability = lookup(providerDescriptor).getCapability(descriptor);
	//		  return new CapableIndexDescriptor(descriptor, capability);
	//	 }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 IndexProviderMap EMPTY = new IndexProviderMap()
	//	 {
	//		  @@Override public IndexProvider lookup(IndexProviderDescriptor descriptor) throws IndexProviderNotFoundException
	//		  {
	//				return IndexProvider.EMPTY;
	//		  }
	//
	//		  @@Override public IndexProvider lookup(String providerDescriptorName) throws IndexProviderNotFoundException
	//		  {
	//				return IndexProvider.EMPTY;
	//		  }
	//
	//		  @@Override public IndexProvider getDefaultProvider()
	//		  {
	//				return IndexProvider.EMPTY;
	//		  }
	//
	//		  @@Override public void accept(Consumer<IndexProvider> visitor)
	//		  {
	//				// yey!
	//		  }
	//	 };
	}

}
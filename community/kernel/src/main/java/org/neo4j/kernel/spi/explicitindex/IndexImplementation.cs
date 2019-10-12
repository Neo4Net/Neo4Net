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
namespace Org.Neo4j.Kernel.spi.explicitindex
{

	using Org.Neo4j.Graphdb;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using TransactionApplier = Org.Neo4j.Kernel.Impl.Api.TransactionApplier;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;

	/// <summary>
	/// An index provider which can create and give access to index transaction state and means of applying
	/// updates to indexes it provides.
	/// An <seealso cref="IndexImplementation"/> is typically tied to one implementation, f.ex.
	/// lucene, http://lucene.apache.org/java.
	/// </summary>
	public interface IndexImplementation : Lifecycle
	{

		 /// <summary>
		 /// Get index implementation root directory based on a store directory </summary>
		 /// <param name="directoryLayout"> database directory structure </param>
		 /// <returns> index implementation root directory </returns>
		 File GetIndexImplementationDirectory( DatabaseLayout directoryLayout );

		 /// <summary>
		 /// Returns a <seealso cref="ExplicitIndexProviderTransaction"/> that keeps transaction state for all
		 /// indexes for a given provider in a transaction.
		 /// </summary>
		 /// <param name="commandFactory"> index command factory to use </param>
		 /// <returns> a <seealso cref="ExplicitIndexProviderTransaction"/> which represents a type of index suitable for the
		 /// given configuration. </returns>
		 ExplicitIndexProviderTransaction NewTransaction( IndexCommandFactory commandFactory );

		 /// <param name="recovery"> indicate recovery </param>
		 /// <returns> an index applier that will get notifications about commands to apply. </returns>
		 TransactionApplier NewApplier( bool recovery );

		 /// <summary>
		 /// Fills in default configuration parameters for indexes provided from this
		 /// index provider. This method will also validate the the configuration is valid to be used
		 /// as index configuration for this provider. </summary>
		 /// <param name="config"> the configuration map to complete with defaults. </param>
		 /// <returns> a <seealso cref="System.Collections.IDictionary"/> filled with decent defaults for an index from
		 /// this index provider. </returns>
		 IDictionary<string, string> FillInDefaults( IDictionary<string, string> config );

		 bool ConfigMatches( IDictionary<string, string> storedConfig, IDictionary<string, string> config );

		 void Force();

		 /// <summary>
		 /// Lists store files that this index provider manages. After this call has been made and until
		 /// the returned <seealso cref="ResourceIterator"/> has been <seealso cref="ResourceIterator.close() closed"/> this
		 /// index provider must guarantee that the list of files stay intact. The files in the list can
		 /// change, but no files may be deleted or added during this period. </summary>
		 /// <returns> list of store files managed by this index provider </returns>
		 /// <exception cref="IOException"> depends on the implementation </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.ResourceIterator<java.io.File> listStoreFiles() throws java.io.IOException;
		 ResourceIterator<File> ListStoreFiles();
	}

}
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
namespace Neo4Net.Kernel.Impl.Api
{
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;

	/// <summary>
	/// Looks up a <seealso cref="ExplicitBatchIndexApplier"/> given a provider name.
	/// </summary>
	public interface ExplicitIndexApplierLookup
	{
		 TransactionApplier NewApplier( string providerName, bool recovery );

		 /// <summary>
		 /// Looks up an <seealso cref="IndexImplementation"/> and calls <seealso cref="IndexImplementation.newApplier(bool)"/> on it.
		 /// </summary>
	}

	 public class ExplicitIndexApplierLookup_Direct : ExplicitIndexApplierLookup
	 {
		  internal readonly ExplicitIndexProvider Provider;

		  public ExplicitIndexApplierLookup_Direct( ExplicitIndexProvider provider )
		  {
				this.Provider = provider;
		  }

		  public override TransactionApplier NewApplier( string providerName, bool recovery )
		  {
				return Provider.getProviderByName( providerName ).newApplier( recovery );
		  }
	 }

}
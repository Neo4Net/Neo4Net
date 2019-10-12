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
namespace Neo4Net.Kernel.api.txstate.auxiliary
{
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;

	/// <summary>
	/// Auxiliary transaction state can be used to attach "opaque" transaction state to a <seealso cref="KernelTransactionImplementation"/> from external sources.
	/// <para>
	/// Those external sources can put whatever transaction-specific data they need into the auxiliary transaction state, but it is up to those external sources,
	/// to ensure that their transaction state is kept up to date. The <seealso cref="KernelTransactionImplementation.getTransactionDataRevision()"/> method can be helpful
	/// for that purpose.
	/// </para>
	/// <para>
	/// The <seealso cref="AuxiliaryTransactionStateProvider"/> is used as a factory of the <seealso cref="AuxiliaryTransactionState"/>, which is created when it is first requested
	/// from the <seealso cref="TxStateHolder.auxiliaryTxState(object)"/>, giving the <seealso cref="getIdentityKey() identity key"/> of the particular auxiliary transaction state
	/// provider.
	/// </para>
	/// </summary>
	public interface AuxiliaryTransactionStateProvider
	{
		 /// <summary>
		 /// Return the <em>identity key</em> that is used to identify the provider, if <seealso cref="TxStateHolder.auxiliaryTxState(object)"/> needs to have a new auxiliary
		 /// transaction state instance created.
		 /// <para>
		 /// Note that this object should have good equals and hashCode implementations, such that it cannot clash with keys from other providers.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> The key object used for identifying the auxiliary transaction state provider. </returns>
		 object IdentityKey { get; }

		 /// <returns> a new instance of the <seealso cref="AuxiliaryTransactionState"/>. </returns>
		 AuxiliaryTransactionState CreateNewAuxiliaryTransactionState();
	}

}
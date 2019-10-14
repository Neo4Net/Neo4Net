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
namespace Neo4Net.@unsafe.Impl.Batchimport.store
{

	using Neo4Net.Kernel.impl.store;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;

	/// <summary>
	/// Exists to allow <seealso cref="IdSequence"/> with specific behaviour relevant to import to be injected into
	/// <seealso cref="CommonAbstractStore.prepareForCommit(AbstractBaseRecord, IdSequence)"/>.
	/// </summary>
	public interface PrepareIdSequence : System.Func<IdSequence, System.Func<long, IdSequence>>
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static PrepareIdSequence of(boolean doubleUnits)
	//	 {
	//		  return doubleUnits ? new SecondaryUnitPrepareIdSequence() : new StorePrepareIdSequence();
	//	 }
	}

}
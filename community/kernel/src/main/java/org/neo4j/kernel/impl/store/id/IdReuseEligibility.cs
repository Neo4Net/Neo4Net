﻿/*
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
namespace Org.Neo4j.Kernel.impl.store.id
{
	using KernelTransactionsSnapshot = Org.Neo4j.Kernel.Impl.Api.KernelTransactionsSnapshot;

	/// <summary>
	/// Deciding whether or not ids are eligible for being released from buffering since being deleted.
	/// </summary>
	public interface IdReuseEligibility
	{

		 bool IsEligible( KernelTransactionsSnapshot snapshot );
	}

	public static class IdReuseEligibility_Fields
	{
		 public static readonly IdReuseEligibility Always = snapshot => true;
	}

}
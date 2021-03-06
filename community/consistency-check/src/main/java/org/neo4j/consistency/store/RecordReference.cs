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
namespace Org.Neo4j.Consistency.store
{
	using Org.Neo4j.Consistency.report;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	public interface RecordReference<RECORD> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 void Dispatch( PendingReferenceCheck<RECORD> reporter );
	}

	 public class RecordReference_SkippingReference<RECORD> : RecordReference<RECORD> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <RECORD extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> RecordReference_SkippingReference<RECORD> skipReference()
		  public static RecordReference_SkippingReference<RECORD> SkipReference<RECORD>() where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		  {
				return Instance;
		  }

		  public override void Dispatch( PendingReferenceCheck<RECORD> reporter )
		  {
				reporter.Skip();
		  }

		  public override string ToString()
		  {
				return "SkipReference";
		  }

		  internal static readonly RecordReference_SkippingReference Instance = new RecordReference_SkippingReference();

		  internal RecordReference_SkippingReference()
		  {
				// singleton
		  }
	 }

}
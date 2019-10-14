/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Batchimport
{

	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using ParallelBatchImporter = Neo4Net.@unsafe.Impl.Batchimport.ParallelBatchImporter;
	using ParallelBatchImporterTest = Neo4Net.@unsafe.Impl.Batchimport.ParallelBatchImporterTest;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Groups = Neo4Net.@unsafe.Impl.Batchimport.input.Groups;

	/// <summary>
	/// Test for <seealso cref="ParallelBatchImporter"/> in an enterprise environment so that enterprise store is used.
	/// </summary>
	public class HighLimitParallelBatchImporterIT : ParallelBatchImporterTest
	{
		 public HighLimitParallelBatchImporterIT( InputIdGenerator inputIdGenerator, System.Func<Groups, IdMapper> idMapper ) : base( inputIdGenerator, idMapper )
		 {
		 }

		 public override RecordFormats Format
		 {
			 get
			 {
				  return HighLimit.RECORD_FORMATS;
			 }
		 }
	}

}
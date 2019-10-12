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
namespace Org.Neo4j.Kernel.impl.transaction
{

	using TransactionHeaderInformation = Org.Neo4j.Kernel.Impl.Api.TransactionHeaderInformation;

	public interface TransactionHeaderInformationFactory
	{
		 TransactionHeaderInformation Create();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 TransactionHeaderInformationFactory DEFAULT = new TransactionHeaderInformationFactory.WithRandomBytes()
	//	 {
	//		  private static final int NO_ID = -1;
	//
	//		  @@Override protected TransactionHeaderInformation createUsing(byte[] additionalHeader)
	//		  {
	//				return new TransactionHeaderInformation(NO_ID, NO_ID, additionalHeader);
	//		  }
	//	 };
	}

	 public abstract class TransactionHeaderInformationFactory_WithRandomBytes : TransactionHeaderInformationFactory
	 {
		  internal const int ADDITIONAL_HEADER_SIZE = 8;

		  public override TransactionHeaderInformation Create()
		  {
				sbyte[] additionalHeader = GenerateAdditionalHeader();
				return CreateUsing( additionalHeader );
		  }

		  protected internal abstract TransactionHeaderInformation CreateUsing( sbyte[] additionalHeader );

		  internal virtual sbyte[] GenerateAdditionalHeader()
		  {
				sbyte[] header = new sbyte[ADDITIONAL_HEADER_SIZE];
				ThreadLocalRandom.current().NextBytes(header);
				return header;
		  }
	 }

}
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
namespace Neo4Net.Configuration
{

	using Neo4Net.Graphdb.config;
	using Neo4Net.Graphdb.config;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;

	/// <summary>
	/// Every class which contains settings should implement this interface to allow the configuration to find the
	/// settings via service loading. Note that service loading requires you to additionally list the service class
	/// under META-INF/services/org.neo4j.configuration.LoadableConfig
	/// </summary>
	public interface LoadableConfig
	{
		 /// <summary>
		 /// Collects settings from implementors which may or may not have descriptions attached to them.
		 /// </summary>
		 /// <returns> a list of the implementing class's ConfigOptions </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.List<ConfigOptions> getConfigOptions()
	//	 {
	//		  ArrayList<ConfigOptions> configOptions = new ArrayList<>();
	//		  for (Field f : getClass().getDeclaredFields())
	//		  {
	//				try
	//				{
	//					 Object publicSetting = f.get(this);
	//					 if (publicSetting instanceof BaseSetting)
	//					 {
	//						  BaseSetting setting = (BaseSetting) publicSetting;
	//
	//						  final Description documentation = f.getAnnotation(Description.class);
	//						  if (documentation != null)
	//						  {
	//								setting.setDescription(documentation.value());
	//						  }
	//
	//						  final DocumentedDefaultValue defValue = f.getAnnotation(DocumentedDefaultValue.class);
	//						  if (defValue != null)
	//						  {
	//								setting.setDocumentedDefaultValue(defValue.value());
	//						  }
	//
	//						  final Deprecated deprecatedAnnotation = f.getAnnotation(Deprecated.class);
	//						  setting.setDeprecated(deprecatedAnnotation != null);
	//
	//						  final ReplacedBy replacedByAnnotation = f.getAnnotation(ReplacedBy.class);
	//						  if (replacedByAnnotation != null)
	//						  {
	//								setting.setReplacement(replacedByAnnotation.value());
	//						  }
	//
	//						  final Internal internalAnnotation = f.getAnnotation(Internal.class);
	//						  setting.setInternal(internalAnnotation != null);
	//
	//						  final Secret secretAnnotation = f.getAnnotation(Secret.class);
	//						  setting.setSecret(secretAnnotation != null);
	//
	//						  final Dynamic dynamicAnnotation = f.getAnnotation(Dynamic.class);
	//						  setting.setDynamic(dynamicAnnotation != null);
	//					 }
	//
	//					 if (publicSetting instanceof SettingGroup)
	//					 {
	//						  SettingGroup setting = (SettingGroup) publicSetting;
	//						  configOptions.add(new ConfigOptions(setting));
	//					 }
	//				}
	//				catch (IllegalAccessException ignored)
	//				{
	//					 // Field is private, ignore it
	//				}
	//		  }
	//		  return configOptions;
	//	 }

		 /// <returns> instances of all classes with loadable configuration options </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.List<LoadableConfig> allConfigClasses()
	//	 {
	//		  return Iterators.stream(ServiceLoader.load(LoadableConfig.class).iterator()).collect(Collectors.toList());
	//	 }
	}

}

using SharePointPagesTranslation.Interfaces;

namespace SharePointPagesTranslation
{
    public class CamlQueries : ICamlQueries
    {

        public string ItemByGuid => @$"<View><Query><Where><Eq><FieldRef Name='UniqueId'/><Value Type='Guid'>{{0}}</Value></Eq></Where></Query></View>";

        public string RecentlyCreatedTranslatedPages => @$"
          <View Scope='RecursiveAll'>
            <ViewFields>
                <FieldRef Name = '_SPTranslationSourceItemId' />
                <FieldRef Name = '_SPIsTranslation' />
                <FieldRef Name = '_SPTranslationLanguage' />
            </ViewFields>
            <Query>
                <Where>
                      <And>
                          <And>
                             <And>
                                <Geq>
                                    <FieldRef Name='{Constants.ModifiedFieldName}' />
                                    <Value Type='DateTime' IncludeTimeValue='TRUE'>{{0}}</Value>
                                </Geq>
                                <IsNotNull>
                                    <FieldRef Name='_SPTranslationSourceItemId' />
                                </IsNotNull>
                             </And>
                              <Eq>
                                 <FieldRef Name='_SPIsTranslation' />
                                  <Value Type='Boolean'>1</Value>
                             </Eq>
                           </And>
                            <Eq>
                                <FieldRef Name='{Constants.AutomateTranslation}' />
                                <Value Type='Boolean'>1</Value>
                           </Eq>
                      </And>
                </Where>
            </Query>
        </View>
        ";

        public string TranslationPagesBySourcePageGuid => @$"
          <View Scope='RecursiveAll'>
            <ViewFields>
                <FieldRef Name = '_SPTranslationSourceItemId' />
                <FieldRef Name = '_SPIsTranslation' />
                <FieldRef Name = '_SPTranslationLanguage' />
            </ViewFields>
            <Query>
                <Where>
                     <Eq>
                        <FieldRef Name='_SPTranslationSourceItemId' />
                        <Value Type='Guid'>{{0}}</Value>
                     </Eq>
                </Where>
            </Query>
        </View>
        ";

        public string RecentlyCreatedPages => @$"
        <View Scope='RecursiveAll'>
            <ViewFields>
                <FieldRef Name = '{Constants.FileRefFieldName}' />
                <FieldRef Name = '{Constants.FileLeafRefFieldName}' />
                <FieldRef Name = '{Constants.EditorFieldName}' />
                <FieldRef Name = '{Constants.ModifiedFieldName}' />
            </ViewFields>
            <Query>
              <Where>
                   <And>
                      <And>
                        <And>
                         <Geq>
                            <FieldRef Name='{Constants.ModifiedFieldName}' />
                            <Value Type='DateTime' IncludeTimeValue='TRUE'>{{0}}</Value>
                         </Geq>
                         <Eq>
                            <FieldRef Name='{Constants.AutomateTranslation}' />
                            <Value Type='Boolean'>1</Value>
                         </Eq>
                      </And>
                        <Eq>
                            <FieldRef Name='_SPIsTranslation' />
                            <Value Type='Boolean'>0</Value>
                        </Eq>
                      </And>
                         <Contains>
                            <FieldRef Name='_UIVersionString' />
                            <Value Type='Text'>.0</Value>
                         </Contains>
                   </And>
               </Where>
            </Query>
        </View>
        ";
    }
}

